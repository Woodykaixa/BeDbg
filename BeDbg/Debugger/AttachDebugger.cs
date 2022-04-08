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


	public override unsafe bool OnException(uint process, uint thread, void* info)
	{
		Console.WriteLine($"Exception {process} {thread}");
		return true;
	}

	public override unsafe bool OnCreateThread(uint process, uint thread, void* info)
	{
		Console.WriteLine($"CreateThread {process} {thread}");
		return true;
	}

	public override unsafe bool OnCreateProcess(uint process, uint thread, void* info)
	{
		Console.WriteLine($"CreateProcess {process} {thread}");
		return true;
	}

	public override unsafe bool OnExitThread(uint process, uint thread, void* info)
	{
		throw new NotImplementedException();
	}

	public override unsafe bool OnExitProcess(uint process, uint thread, void* info)
	{
		throw new NotImplementedException();
	}

	public override unsafe bool OnLoadDll(uint process, uint thread, void* info)
	{
		throw new NotImplementedException();
	}

	public override unsafe bool OnUnloadDll(uint process, uint thread, void* info)
	{
		throw new NotImplementedException();
	}

	public override unsafe bool OnOutputDebugString(uint process, uint thread, void* info)
	{
		throw new NotImplementedException();
	}

	public override unsafe bool OnRip(uint process, uint thread, void* info)
	{
		throw new NotImplementedException();
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