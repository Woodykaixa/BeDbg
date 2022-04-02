using BeDbg.Api;

namespace BeDbg.Debugger;

/// <summary>
/// Debug a process by attaching it
/// </summary>
public class AttachDebugger : BaseDebugger
{
	public AttachDebugger(int pid) : base(pid, 0)
	{
		ApiError.Clear();
		TargetHandle = BeDbg64.AttachProcess(pid).ToInt64();
		if (TargetHandle == 0)
		{
			throw ApiError.FormatError();
		}

		ReadProcessMemoryPages();
		ReadProcessModules();
	}

	~AttachDebugger()
	{
		ApiError.Clear();

		if (!BeDbg64.DetachProcess(new IntPtr(TargetHandle)))
		{
			throw ApiError.FormatError();
		}
	}
}