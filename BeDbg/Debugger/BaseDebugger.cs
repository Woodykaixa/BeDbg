using System.Diagnostics;
using BeDbg.Api;
using BeDbg.Dto;
using BeDbg.Models;
using Iced.Intel;

namespace BeDbg.Debugger;

/// <summary>
/// <para>
/// Base debugger type defines common operations and properties for debugging. Inherit this class
/// to implement a debugger class and hold it in memory database, all network request works on it.
/// </para>
///
/// <para>
/// If you inherit this class, make sure to initialize debugger resources in constructor and release
/// in destructor. This behavior looks like RAII in C++.
/// </para>
///
/// <para>
/// Classes inherited this class: <see cref="CreateDebugger"/>, <see cref="AttachDebugger"/>
/// </para>
/// </summary>
public abstract class BaseDebugger : DebugEventHandler
{
	// Use this CancellationToken to control all threads created by debugger
	protected readonly CancellationTokenSource CancellationTokenSource = new();
	public CancellationToken CancellationToken => CancellationTokenSource.Token;

	public Queue<DebuggerEvent> DebuggerEventList = new(64);

	private bool _firstException = true; // When debugger handles the first exception, it sends a "programReady" event

	public DateTime StartTime { get; } = DateTime.Now;
	public int TargetPid { get; internal set; }

	public long TargetHandle { get; internal set; }

	public List<BeDbg64.ProcessMemoryBlockInformation> MemPages { get; } = new(128);

	protected Task? _debugLoop;
	private ulong _rip = 0;

	protected bool DoDebugLoop = true;

	public Dictionary<uint, ProcessModel> Processes = new(16);
	public Dictionary<long, RuntimeModuleModel> Modules = new(32);


	public DebuggingProcess TargetProcess => new()
	{
		AttachTime = StartTime,
		Handle = TargetHandle,
		Id = TargetPid
	};

	public BeDbg64.Registers GetRegisters(ulong thread)
	{
		var process = Processes[(uint) TargetPid];
		var threadHandle = process.Threads[process.MainThread].Handle;
		unsafe
		{
			var registers = stackalloc BeDbg64.Registers[1];
			BeDbg64.GetThreadRegisters(threadHandle, registers);
			return *registers;
		}
	}

	~BaseDebugger()
	{
		CancellationTokenSource.Cancel();
	}

	public void OnProgramReady()
	{
		MemPages.Clear();
		MemPages.AddRange(BeDbg64.QueryProcessMemoryPages(new IntPtr(TargetHandle)));
		for (var i = 0; i < MemPages.Count; i++)
		{
			var memPage = MemPages[i];
			if (memPage.BaseAddress == 0x7FFE0000)
			{
				memPage.Info = "KUSER_SHARED_DATA";
				continue;
			}
		}
	}

	public override unsafe bool OnException(uint process, uint thread, void* info)
	{
		var exceptionRecord = (ExceptionDebugInfo*) info;
		if (_firstException)
		{
			OnProgramReady();
			DebuggerEventList.Enqueue(new DebuggerEvent
			{
				Event = "programReady"
			});
		}

		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "exception",
			Payload = new ExceptionPayload
			{
				Process = process,
				Thread = thread,
				ExceptionAddress = exceptionRecord->ExceptionRecord.ExceptionAddress.ToInt64(),
				ExceptionCode = exceptionRecord->ExceptionRecord.ExceptionCode,
				ExceptionFlag = exceptionRecord->ExceptionRecord.ExceptionFlags,
				FirstChance = exceptionRecord->dwFirstChance
			}
		});
		return true;
	}

	public override unsafe bool OnCreateThread(uint process, uint thread, void* info)
	{
		var threadInfo = (CreateThreadDebugInfo*) info;
		Processes[process].Threads[thread] = new ThreadModel(thread, threadInfo->hThread, threadInfo->lpStartAddress,
			threadInfo->lpThreadLocalBase);
		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "createThread",
			Payload = new CreateThreadPayload
			{
				Process = process,
				Thread = thread,
				StartAddress = threadInfo->lpStartAddress.ToInt64(),
				ThreadLocalBase = threadInfo->lpThreadLocalBase.ToInt64()
			}
		});
		return true;
	}

	public override unsafe bool OnCreateProcess(uint process, uint thread, void* info)
	{
		var processInfo = (CreateProcessDebugInfo*) info;
		var currentProcess = new ProcessModel
		{
			Id = process,
			MainThread = thread,
			Handle = processInfo->hProcess,

			Threads =
			{
				[thread] = new ThreadModel(thread, processInfo->hThread, processInfo->lpStartAddress,
					processInfo->lpThreadLocalBase)
			}
		};

		var module = new RuntimeModuleModel(processInfo->hFile, processInfo->lpImageName, processInfo->fUnicode)
		{
			ImageBase = processInfo->lpBaseOfImage,
			DebugInfoOffset = processInfo->dwDebugInfoFileOffset,
			DebugInfoSize = processInfo->nDebugInfoSize,
		};

		Processes[process] = currentProcess;
		Modules[processInfo->lpBaseOfImage.ToInt64()] = module;

		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "createProcess",
			Payload = new CreateProcessPayload
			{
				Process = process,
				Thread = thread,
				BaseOfImage = processInfo->lpBaseOfImage.ToInt64(),
				ThreadLocalBase = processInfo->lpThreadLocalBase.ToInt64(),
				StartAddress = processInfo->lpStartAddress.ToInt64()
			}
		});
		return true;
	}

	public override unsafe bool OnExitThread(uint process, uint thread, void* info)
	{
		var exitCode = ((ExitThreadDebugInfo*) info)->dwExitCode;
		Processes[process].Threads.Remove(thread);
		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "exitThread",
			Payload = new ExitThreadPayload
			{
				Process = process,
				Thread = thread,
				ExitCode = exitCode
			}
		});
		return true;
	}

	public override unsafe bool OnExitProcess(uint process, uint thread, void* info)
	{
		var exitCode = ((ExitProcessDebugInfo*) info)->dwExitCode;
		Processes.Remove(process);
		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "exitProcess",
			Payload = new ExitProcessPayload
			{
				Process = process,
				Thread = thread,
				ExitCode = exitCode
			}
		});

		if (Processes.Count != 0)
		{
			return true;
		}

		DoDebugLoop = false;
		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "exitProgram",
		});

		return true;
	}

	public override unsafe bool OnLoadDll(uint process, uint thread, void* info)
	{
		var dll = (LoadDllDebugInfo*) info;
		var module = new RuntimeModuleModel(dll->hFile, dll->lpImageName, dll->fUnicode)
		{
			DebugInfoOffset = dll->dwDebugInfoFileOffset,
			DebugInfoSize = dll->fUnicode,
			ImageBase = dll->lpBaseOfDll
		};
		Modules[dll->lpBaseOfDll.ToInt64()] = module;
		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "loadDll",
			Payload = new LoadDllPayload
			{
				Process = process,
				Thread = thread,
				BaseOfDll = dll->lpBaseOfDll.ToInt64()
			}
		});
		return true;
	}

	public override unsafe bool OnUnloadDll(uint process, uint thread, void* info)
	{
		var unload = (UnloadDllDebugInfo*) info;
		Modules.Remove(unload->lpBaseOfDll.ToInt64());
		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "unloadDll",
			Payload = new UnloadDllPayload
			{
				Process = process,
				Thread = thread,
				BaseOfDll = unload->lpBaseOfDll.ToInt64()
			}
		});
		return true;
	}

	public override unsafe bool OnOutputDebugString(uint process, uint thread, void* info)
	{
		// TODO: handle OutputDebugString Event
		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "unloadDll",
			Payload = new OutputDebugStringPayload
			{
				Process = process,
				Thread = thread,
			}
		});
		return true;
	}

	public override unsafe bool OnRip(uint process, uint thread, void* info)
	{
		var rip = (RipInfo*) info;
		Console.Error.WriteLine(
			$"Rip Error: Process {process}, Thread {thread}, Code {rip->dwError}, Type {rip->dwType}");
		DebuggerEventList.Enqueue(new DebuggerEvent
		{
			Event = "rip",
			Payload = new RipPayload
			{
				Process = process,
				Thread = thread,
				Error = rip->dwError,
				Type = rip->dwType
			}
		});
		return false;
	}

	// protected void ReadProcessModules()
	// {
	// 	// var modules = BeDbg64.QueryProcessModules(new IntPtr(TargetHandle));
	// 	// Modules.Clear();
	// 	// Modules.AddRange(modules.Select(m => new RuntimeModuleModel()));
	// }
	//
	// protected void ReadProcessMemoryPages()
	// {
	// 	MemPages.Clear();
	// 	var pages = BeDbg64.QueryProcessMemoryPages(new IntPtr(TargetHandle));
	// 	MemPages.AddRange(pages);
	// }

	public IEnumerable<InstructionModel> Disassemble(ulong address, uint size)
	{
		var buffer = new byte[size];
		Kernel.ReadProcessMemory(new IntPtr(TargetHandle), new IntPtr((long) address), buffer, size, out var read);
		var decoder = Decoder.Create(64, new ByteArrayCodeReader(buffer));
		decoder.IP = (ulong) address;
		var endRip = decoder.IP + (uint) read;
		var instructions = new List<Instruction>();
		while (decoder.IP < endRip)
			instructions.Add(decoder.Decode());
		_rip = decoder.IP;

		var formatter = new NasmFormatter()
		{
			Options =
			{
				DigitSeparator = null,
				FirstOperandCharIndex = 0,
				AddLeadingZeroToHexNumbers = false,
				AlwaysShowSegmentRegister = true,
				NasmShowSignExtendedImmediateSize = true
			}
		};
		var output = new StringOutput();
		return instructions.Where(instr => !instr.IsInvalid).Select(instr =>
		{
			formatter.Format(instr, output);
			return new InstructionModel()
			{
				Ip = instr.IP,
				Text = output.ToStringAndReset()
			};
		});
	}

	protected void DebugLoop()
	{
		// Process.EnterDebugMode();
		while (DoDebugLoop)
		{
			if (!DebugLoopWaitEvent(CallbackHandle))
			{
				throw ApiError.FormatError();
			}
		}
	}

	protected void StartDebugLoop()
	{
		// Kernel.DebugActiveProcess(TargetPid);
		DebugLoop();
		// Kernel.DebugActiveProcessStop(TargetPid);
	}
}