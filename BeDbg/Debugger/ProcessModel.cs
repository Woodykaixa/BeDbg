namespace BeDbg.Debugger;

/// <summary>
/// <see cref="BaseDebugger"/> uses this class to interact with Windows Process
/// </summary>
public class ProcessModel
{
	public uint Id { get; set; }
	public uint MainThread { get; set; }
	public Dictionary<uint, ThreadModel> Threads = new(16);
	public IntPtr Handle { get; set; }
}