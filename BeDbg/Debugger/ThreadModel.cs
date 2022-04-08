namespace BeDbg.Debugger;

/// <summary>
/// <see cref="BaseDebugger"/> uses this class to interact with Windows Thread
/// </summary>
public class ThreadModel
{
	public uint Id { get; set; }
	public IntPtr Address { get; set; }
	public IntPtr Entry { get; set; }
	public IntPtr Handle { get; set; }

	public ThreadModel(uint id, IntPtr handle, IntPtr entry, IntPtr baseAddress)
	{
		Id = id;
		Handle = handle;
		Entry = entry;
		Address = baseAddress;
	}
}