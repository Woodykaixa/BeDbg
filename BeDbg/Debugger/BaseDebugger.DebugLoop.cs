using System.ComponentModel;
using BeDbg.Api;

namespace BeDbg.Debugger;

internal struct DebugContinueData
{
	public int ContinueThread;
	public ManualResetEvent ContinueEvent;

	public DebugContinueData()
	{
		ContinueThread = 0;
		ContinueEvent = new ManualResetEvent(false);
	}
}

public abstract partial class BaseDebugger
{
	protected bool DoDebugLoop = true;
	protected Task? DebugLoopThread;
	private DebugContinueData _continueData = new();


	protected void DebugLoop()
	{
		while (DoDebugLoop)
		{
			var result = DebugLoopWaitEvent(CallbackHandle);
			switch (result)
			{
				case DebugContinueStatus.NotHandled:
					throw ApiError.FormatError();
				case DebugContinueStatus.WaitForExplicitContinue:
				{
					_continueData.ContinueEvent.Reset();

					_continueData.ContinueEvent.WaitOne();
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
		DebugLoop();
	}
}