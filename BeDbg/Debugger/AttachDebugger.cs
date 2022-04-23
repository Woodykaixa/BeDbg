using BeDbg.Api;

namespace BeDbg.Debugger;

/// <summary>
/// Debug a process by attaching it
/// </summary>
public class AttachDebugger : BaseDebugger
{
	public AttachDebugger(int pid)
	{
		ApiError.Clear();
		var handle = BeDbg64.AttachProcess(pid).ToInt64();
		if (handle == 0)
		{
			throw ApiError.FormatError();
		}

		TargetHandle = handle;
		TargetPid = pid;
		_debugLoop = Task.Factory.StartNew(() =>
		{
			Kernel.DebugActiveProcess(pid);
			StartDebugLoop();
			Kernel.DebugActiveProcessStop(pid);
		}, TaskCreationOptions.LongRunning);
	}

	public override void OnRelease()
	{
		base.OnRelease();
		ApiError.Clear();
		// Kernel.DebugActiveProcessStop(TargetPid);

		if (!BeDbg64.DetachProcess(new IntPtr(TargetHandle)))
		{
			throw ApiError.FormatError();
		}
	}
}