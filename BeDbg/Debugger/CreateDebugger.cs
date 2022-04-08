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

	public static CreateDebugger CreateFromFile(string filename, string command, string? env, string? cwd)
	{
		ApiError.Clear();
		var pid = startProcess(filename, command, env, cwd);
		if (pid == 0)
		{
			throw ApiError.FormatError();
		}

		var handle = BeDbg64.AttachProcess(pid).ToInt64();
		if (handle == 0)
		{
			throw ApiError.FormatError();
		}

		return new CreateDebugger(pid, handle);
	}

	private CreateDebugger(int pid, long targetHandle) : base(pid, targetHandle)
	{
		// ReadProcessModules();
		// ReadProcessMemoryPages();
		
		StartDebugLoop();
	}

	public override unsafe bool OnException(uint process, uint thread, void* info)
	{
		throw new NotImplementedException();
	}

	public override unsafe bool OnCreateThread(uint process, uint thread, void* info)
	{
		throw new NotImplementedException();
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

	~CreateDebugger()
	{
		Kernel.TerminateProcess(new IntPtr(TargetHandle), 0);
	}
}