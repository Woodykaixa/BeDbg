using BeDbg.Api;
using BeDbg.Models;

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
public abstract class BaseDebugger
{
	public DateTime StartTime { get; }
	public int TargetPid { get; internal set; }
	public long TargetHandle { get; internal set; }
	public List<BeDbg64.ProcessModuleInformation> Modules { get; }
	public List<BeDbg64.ProcessMemoryBlockInformation> MemPages { get; }

	protected BaseDebugger(int pid, long targetHandle)
	{
		StartTime = DateTime.Now;
		TargetPid = pid;
		TargetHandle = targetHandle;
		Modules = new List<BeDbg64.ProcessModuleInformation>(128);
		MemPages = new List<BeDbg64.ProcessMemoryBlockInformation>(128);
	}

	public DebuggingProcess TargetProcess => new()
	{
		AttachTime = StartTime,
		Handle = TargetHandle,
		Id = TargetPid
	};

	protected void ReadProcessModules()
	{
		var modules = BeDbg64.QueryProcessModules(new IntPtr(TargetHandle));
		Modules.Clear();
		Modules.AddRange(modules);
	}

	protected void ReadProcessMemoryPages()
	{
		MemPages.Clear();
		var pages = BeDbg64.QueryProcessMemoryPages(new IntPtr(TargetHandle));
		MemPages.AddRange(pages);
	}
}