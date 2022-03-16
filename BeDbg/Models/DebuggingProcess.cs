namespace BeDbg.Models;

public class DebuggingProcess
{
	public DateTime AttachTime { get; set; }
	public int Id { get; set; }
	public IntPtr Handle { get; set; }
}