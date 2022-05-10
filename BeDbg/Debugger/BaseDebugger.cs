using System.ComponentModel;
using System.Diagnostics;
using BeDbg.Api;
using BeDbg.Dto;
using BeDbg.Models;
using Iced.Intel;

namespace BeDbg.Debugger;

internal struct DebugContinueData
{
	public bool DebugContinue;
	public int ContinueThread;
}

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

	public List<DebuggerEvent> DebuggerEventList = new(64);
	public object DebuggerEventListLock = new();

	private bool _firstException = true; // When debugger handles the first exception, it sends a "programReady" event

	public bool WaitContinueCommand = false;

	public DateTime StartTime { get; } = DateTime.Now;
	public int TargetPid { get; internal set; }

	public long TargetHandle { get; internal set; }

	public List<BeDbg64.ProcessMemoryBlockInformation> MemPages { get; } = new(128);

	protected Task? _debugLoop;
	private ulong _rip = 0;

	private DebugContinueData _continueData = new();

	protected bool DoDebugLoop = true;

	public Dictionary<uint, ProcessModel> Processes = new(16);
	public Dictionary<long, RuntimeModuleModel> Modules = new(32);

	public DebuggingProcess TargetProcess => new()
	{
		AttachTime = StartTime,
		Handle = TargetHandle,
		Id = TargetPid
	};

	protected void EmitDebuggerEvent(DebuggerEvent e)
	{
		lock (DebuggerEventListLock)
		{
			DebuggerEventList.Add(e);
		}
	}

	public BeDbg64.Registers GetRegisters(ulong thread)
	{
		var process = Processes[(uint) TargetPid];
		var threadHandle = process.Threads[process.MainThread].Handle;
		// if (Kernel.SuspendThread(threadHandle) == uint.MaxValue)
		// {
		// 	throw new Win32Exception();
		// }

		unsafe
		{
			var registers = stackalloc BeDbg64.Registers[1];
			BeDbg64.GetThreadRegisters(threadHandle, registers);
			// if (Kernel.ResumeThread(threadHandle) == uint.MaxValue)
			// {
			// 	throw new Win32Exception();
			// }

			return *registers;
		}
	}

	public override void OnRelease()
	{
		CancellationTokenSource.Cancel();
		DoDebugLoop = false;
		base.OnRelease();
	}

	public void OnProgramReady()
	{
		MemPages.Clear();
		MemPages.AddRange(BeDbg64.QueryProcessMemoryPages(new IntPtr(TargetHandle)));
		WaitContinueCommand = true;
		var process = Processes[(uint) TargetPid];
		Kernel.SuspendThread(process.Threads[process.MainThread].Handle);

		EmitDebuggerEvent(new DebuggerEvent
		{
			Event = "programReady"
		});
	}

	public override unsafe DebugContinueStatus OnException(uint process, uint thread, void* info)
	{
		var exceptionRecord = (ExceptionDebugInfo*) info;
		if (_firstException)
		{
			_firstException = false;
			OnProgramReady();
		}

		EmitDebuggerEvent(new DebuggerEvent
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

		return DebugContinueStatus.WaitForExplicitContinue;
	}

	public override unsafe DebugContinueStatus OnCreateThread(uint process, uint thread, void* info)
	{
		var threadInfo = (CreateThreadDebugInfo*) info;
		Processes[process].Threads[thread] = new ThreadModel(thread, threadInfo->hThread, threadInfo->lpStartAddress,
			threadInfo->lpThreadLocalBase);
		EmitDebuggerEvent(new DebuggerEvent
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
		return DebugContinueStatus.AutoContinue;
	}

	public override unsafe DebugContinueStatus OnCreateProcess(uint process, uint thread, void* info)
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

		EmitDebuggerEvent(new DebuggerEvent
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
		return DebugContinueStatus.AutoContinue;
	}

	public override unsafe DebugContinueStatus OnExitThread(uint process, uint thread, void* info)
	{
		var exitCode = ((ExitThreadDebugInfo*) info)->dwExitCode;
		Processes[process].Threads.Remove(thread);
		EmitDebuggerEvent(new DebuggerEvent
		{
			Event = "exitThread",
			Payload = new ExitThreadPayload
			{
				Process = process,
				Thread = thread,
				ExitCode = exitCode
			}
		});
		return DebugContinueStatus.AutoContinue;
	}

	public override unsafe DebugContinueStatus OnExitProcess(uint process, uint thread, void* info)
	{
		var exitCode = ((ExitProcessDebugInfo*) info)->dwExitCode;
		Processes.Remove(process);
		EmitDebuggerEvent(new DebuggerEvent
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
			return DebugContinueStatus.AutoContinue;
		}

		DoDebugLoop = false;
		EmitDebuggerEvent(new DebuggerEvent
		{
			Event = "exitProgram",
		});

		return DebugContinueStatus.AutoContinue;
	}

	public override unsafe DebugContinueStatus OnLoadDll(uint process, uint thread, void* info)
	{
		var dll = (LoadDllDebugInfo*) info;
		var module = new RuntimeModuleModel(dll->hFile, dll->lpImageName, dll->fUnicode)
		{
			DebugInfoOffset = dll->dwDebugInfoFileOffset,
			DebugInfoSize = dll->fUnicode,
			ImageBase = dll->lpBaseOfDll
		};
		Modules[dll->lpBaseOfDll.ToInt64()] = module;
		EmitDebuggerEvent(new DebuggerEvent
		{
			Event = "loadDll",
			Payload = new LoadDllPayload
			{
				Process = process,
				Thread = thread,
				BaseOfDll = dll->lpBaseOfDll.ToInt64()
			}
		});
		return DebugContinueStatus.AutoContinue;
	}

	public override unsafe DebugContinueStatus OnUnloadDll(uint process, uint thread, void* info)
	{
		var unload = (UnloadDllDebugInfo*) info;
		Modules.Remove(unload->lpBaseOfDll.ToInt64());
		EmitDebuggerEvent(new DebuggerEvent
		{
			Event = "unloadDll",
			Payload = new UnloadDllPayload
			{
				Process = process,
				Thread = thread,
				BaseOfDll = unload->lpBaseOfDll.ToInt64()
			}
		});
		return DebugContinueStatus.AutoContinue;
	}

	public override unsafe DebugContinueStatus OnOutputDebugString(uint process, uint thread, void* info)
	{
		// TODO: handle OutputDebugString Event
		EmitDebuggerEvent(new DebuggerEvent
		{
			Event = "outputDebugString",
			Payload = new OutputDebugStringPayload
			{
				Process = process,
				Thread = thread,
			}
		});
		return DebugContinueStatus.AutoContinue;
	}

	public override unsafe DebugContinueStatus OnRip(uint process, uint thread, void* info)
	{
		var rip = (RipInfo*) info;
		Console.Error.WriteLine(
			$"Rip Error: Process {process}, Thread {thread}, Code {rip->dwError}, Type {rip->dwType}");
		EmitDebuggerEvent(new DebuggerEvent
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
		return DebugContinueStatus.NotHandled;
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

	public void StepIn(int threadId)
	{
		var process = Processes[(uint) TargetPid];
		var thread = process.Threads[(uint) threadId];
		var flag = BeDbg64.GetThreadContextFlag(thread.Handle);
		BeDbg64.SetThreadContextFlag(thread.Handle, flag | 0x100);
		Continue(threadId);
	}

	public void Continue(int threadId)
	{
		_continueData.ContinueThread = threadId;
		_continueData.DebugContinue = true;
	}

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
			var result = DebugLoopWaitEvent(CallbackHandle);
			switch (result)
			{
				case DebugContinueStatus.NotHandled:
					throw ApiError.FormatError();
				case DebugContinueStatus.WaitForExplicitContinue:
				{
					_continueData.DebugContinue = false;
					var waitDebugContinueTask = Task.Factory.StartNew(() =>
					{
						while (true)
						{
							if (_continueData.DebugContinue)
							{
								break;
							}

							Thread.Sleep(1000);
						}
					}, CancellationToken);
					waitDebugContinueTask.Wait(CancellationToken);
					if (Kernel.ContinueDebugEvent(TargetPid, _continueData.ContinueThread, Kernel.DbgContinue) == false)
					{
						throw new Win32Exception(Kernel.GetLastError());
					}

					Kernel.ResumeThread(Processes[(uint) TargetPid].Threads[(uint) _continueData.ContinueThread]
						.Handle);


					break;
				}
				case DebugContinueStatus.AutoContinue:
					break;
				default:
					throw new ArgumentOutOfRangeException();
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