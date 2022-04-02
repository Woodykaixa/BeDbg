using System.Runtime.InteropServices;
using BeDbg.Api;

namespace BeDbg.Debugger;

/// <summary>
/// Debug a process created by CreateProcess
/// </summary>
public class CreateDebugger : BaseDebugger
{
	[DllImport(InteropConfig.Api64, EntryPoint = "StartProcess", CharSet = CharSet.Unicode)]
	private static extern int startProcess(string filename, string command, string? environment,
		string? workingDirectory);

	public CreateDebugger(string filename, string command, string? environment, string? workingDir) : base(0, 0)
	{
		ApiError.Clear();
		TargetPid = startProcess(filename, command, environment, workingDir);
		if (TargetPid == 0)
		{
			throw ApiError.FormatError();
		}

		TargetHandle = BeDbg64.AttachProcess(TargetPid).ToInt64();
		if (TargetHandle == 0)
		{
			throw ApiError.FormatError();
		}
		ReadProcessMemoryPages();
		ReadProcessModules();
	}

	~CreateDebugger()
	{
		Kernel.TerminateProcess(new IntPtr(TargetHandle), 0);
	}
}