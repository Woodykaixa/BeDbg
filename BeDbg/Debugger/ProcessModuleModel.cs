namespace BeDbg.Debugger;

public class ProcessModuleModel
{
	public string Name { get; }
	public ulong Entry { get; }
	public uint Size { get; }
	public ulong Base { get; }
	public string Info { get; set; }

	public ProcessModuleModel(string name, ulong entry, uint size, ulong baseAddress)
	{
		Name = name;
		Entry = entry;
		Size = size;
		Base = baseAddress;
		Info = string.Empty;
	}
}