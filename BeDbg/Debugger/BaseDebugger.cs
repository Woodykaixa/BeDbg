using System.Diagnostics;
using BeDbg.Api;
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
	public DateTime StartTime { get; } = DateTime.Now;
	public int TargetPid { get; internal set; }

	public long TargetHandle { get; internal set; }

	// public List<BeDbg64.ProcessModuleInformation> Modules { get; } = new(128);
	public List<BeDbg64.ProcessMemoryBlockInformation> MemPages { get; } = new(128);

	protected Task? _debugLoop;
	private ulong _rip = 0;

	protected bool DoDebugLoop = true;

	public Dictionary<uint, ProcessModel> Processes = new(16);
	public Dictionary<long, RuntimeModuleModel> Modules = new(32);

	protected BaseDebugger(int pid, long targetHandle)
	{
		TargetPid = pid;
		TargetHandle = targetHandle;
	}

	public DebuggingProcess TargetProcess => new()
	{
		AttachTime = StartTime,
		Handle = TargetHandle,
		Id = TargetPid
	};


	public override unsafe bool OnException(uint process, uint thread, void* info)
	{
		var exceptionRecord = (ExceptionDebugInfo*) info;
		Console.WriteLine(
			$"Exception {process} {thread} {exceptionRecord->ExceptionRecord.ExceptionAddress} {exceptionRecord->ExceptionRecord.ExceptionCode}");
		return true;
	}

	public override unsafe bool OnCreateThread(uint process, uint thread, void* info)
	{
		var threadInfo = (CreateThreadDebugInfo*) info;
		Processes[process].Threads[thread] = new ThreadModel(thread, threadInfo->hThread, threadInfo->lpStartAddress,
			threadInfo->lpThreadLocalBase);
		Console.WriteLine($"CreateThread {process} {thread}");
		return true;
	}

	public override unsafe bool OnCreateProcess(uint process, uint thread, void* info)
	{
		var processInfo = (CreateProcessDebugInfo*) info;
		Processes[process] = new ProcessModel
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

		Modules[processInfo->lpBaseOfImage.ToInt64()] =
			new RuntimeModuleModel(processInfo->hFile, processInfo->lpImageName, processInfo->fUnicode)
			{
				ImageBase = processInfo->lpBaseOfImage,
				DebugInfoOffset = processInfo->dwDebugInfoFileOffset,
				DebugInfoSize = processInfo->nDebugInfoSize,
			};
		Console.WriteLine($"CreateProcess {process} {thread}");
		return true;
	}

	public override unsafe bool OnExitThread(uint process, uint thread, void* info)
	{
		var exitCode = ((ExitThreadDebugInfo*) info)->dwExitCode;
		Processes[process].Threads.Remove(thread);
		Console.WriteLine($"ExitThread {process} {thread} {exitCode}");
		return true;
	}

	public override unsafe bool OnExitProcess(uint process, uint thread, void* info)
	{
		var exitCode = ((ExitThreadDebugInfo*) info)->dwExitCode;
		Processes.Remove(process);
		Console.WriteLine($"ExitProcess {process} {thread} {exitCode}");
		if (Processes.Count == 0)
		{
			DoDebugLoop = false;
			Console.WriteLine("End Debug Loop");
		}

		return true;
	}

	public override unsafe bool OnLoadDll(uint process, uint thread, void* info)
	{
		var dll = (LoadDllDebugInfo*) info;
		Modules[dll->lpBaseOfDll.ToInt64()] = new RuntimeModuleModel(dll->hFile, dll->lpImageName, dll->fUnicode)
		{
			DebugInfoOffset = dll->dwDebugInfoFileOffset,
			DebugInfoSize = dll->fUnicode,
			ImageBase = dll->lpBaseOfDll
		};
		return true;
	}

	public override unsafe bool OnUnloadDll(uint process, uint thread, void* info)
	{
		var unload = (UnloadDllDebugInfo*) info;
		Modules.Remove(unload->lpBaseOfDll.ToInt64());
		return true;
	}

	public override unsafe bool OnOutputDebugString(uint process, uint thread, void* info)
	{
		return true;
	}

	public override unsafe bool OnRip(uint process, uint thread, void* info)
	{
		var rip = (RipInfo*) info;
		Console.Error.WriteLine(
			$"Rip Error: Process {process}, Thread {thread}, Code {rip->dwError}, Type {rip->dwType}");
		return false;
	}

	protected void ReadProcessModules()
	{
		// var modules = BeDbg64.QueryProcessModules(new IntPtr(TargetHandle));
		// Modules.Clear();
		// Modules.AddRange(modules.Select(m => new RuntimeModuleModel()));
	}

	protected void ReadProcessMemoryPages()
	{
		MemPages.Clear();
		var pages = BeDbg64.QueryProcessMemoryPages(new IntPtr(TargetHandle));
		MemPages.AddRange(pages);
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
		return instructions.Where(instr => instr.IsInvalid).Select(instr =>
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
		Process.EnterDebugMode();
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
		DebugLoop();
		// TODO: use another thread
		// if (_debugLoop != null)
		// {
		// 	return;
		// }
		//
		// _debugLoop = new Task(DebugLoop);
	}
}