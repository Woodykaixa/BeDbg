using BeDbg.Api;
using BeDbg.Dto;

namespace BeDbg.Debugger;

public class EmitDebuggerEventArgs : EventArgs
{
}

public abstract partial class BaseDebugger : DebugEventHandler
{
	private bool _firstException = true; // When debugger handles the first exception, it sends a "programReady" event

	public event EventHandler<EmitDebuggerEventArgs>? EmitDebuggerEventHandler;

	public List<DebuggerEvent> DebuggerEventList = new(64);
	public object DebuggerEventListLock = new();

	protected virtual void OnEmitDebuggerEvent(EmitDebuggerEventArgs e)
	{
		var handler = EmitDebuggerEventHandler;
		handler?.Invoke(this, e);
	}

	protected void EmitDebuggerEvent(DebuggerEvent e)
	{
		lock (DebuggerEventListLock)
		{
			DebuggerEventList.Add(e);
			OnEmitDebuggerEvent(new EmitDebuggerEventArgs());
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

		var exceptionEvent = exceptionRecord->ExceptionRecord.ExceptionCode switch
		{
			0x80000003 => "breakpoint",
			0x80000004 => "singleStep",
			_ => "exception"
		};

		EmitDebuggerEvent(new DebuggerEvent
		{
			Event = exceptionEvent,
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
}