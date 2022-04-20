using BeDbg.Api;
using BeDbg.Debugger;
using BeDbg.Filters;
using BeDbg.Models;

namespace BeDbg.Services;

public class DebugService
{
	private readonly List<BaseDebugger> _debuggers;

	public DebugService()
	{
		_debuggers = new List<BaseDebugger>(16);
	}

	public BaseDebugger? FindOneByPid(int pid)
	{
		return _debuggers.Find(dbg => dbg.TargetPid == pid);
	}

	public BaseDebugger? FindOneByIndex(int index)
	{
		if (index < 0 || index >= _debuggers.Count)
		{
			return null;
		}

		return _debuggers[index];
	}

	public DebuggingProcess CreateDebugProcess(string file, string command, string? environment,
		string? workingDir)
	{
		var debugger = new CreateDebugger(file, command, environment, workingDir);
		_debuggers.Add(debugger);

		return debugger.TargetProcess;
	}

	public DebuggingProcess AttachProcess(int pid)
	{
		if (FindOneByPid(pid) != null)
		{
			throw new Exception($"process {pid} already attached");
		}

		var debugger = new AttachDebugger(pid);
		_debuggers.Add(debugger);
		return debugger.TargetProcess;
	}

	public IEnumerable<DebuggingProcess> ListDebuggingProcesses() => _debuggers.Select(dbg => dbg.TargetProcess);

	public bool StopDebugProcessByPid(int pid)
	{
		var debugger = FindOneByPid(pid);
		if (debugger == null)
		{
			return true;
		}

		_debuggers.Remove(debugger);

		return true;
	}
}

public static class DebugServiceExtension
{
	public static IServiceCollection UseDebugService(
		this IServiceCollection service)
	{
		return service.AddSingleton(new DebugService());
	}
}