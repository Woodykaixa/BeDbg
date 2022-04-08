using BeDbg.Api;

namespace BeDbg.Debugger;

/// <summary>
/// Debug a process by attaching it
/// </summary>
public class AttachDebugger : BaseDebugger
{
	public static AttachDebugger Attach(int pid)
	{
		ApiError.Clear();
		var handle = BeDbg64.AttachProcess(pid).ToInt64();
		Kernel.DebugActiveProcess(pid);
		if (handle == 0)
		{
			throw ApiError.FormatError();
		}

		return new AttachDebugger(pid, handle);
	}

	private AttachDebugger(int pid, long targetHandle) : base(pid, targetHandle)
	{
		ReadProcessModules();
		// ReadProcessMemoryPages();
		StartDebugLoop();
	}


	~AttachDebugger()
	{
		ApiError.Clear();
		Kernel.DebugActiveProcessStop(TargetPid);

		if (!BeDbg64.DetachProcess(new IntPtr(TargetHandle)))
		{
			throw ApiError.FormatError();
		}
	}
}