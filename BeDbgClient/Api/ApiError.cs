using System.Runtime.InteropServices;

namespace BeDbgClient.Api;

public class ApiError
{
	public enum ErrorModule
	{
		NoError,
		System,
		Asm,
		Unknown
	}

	[DllImport(Config.LibName, EntryPoint = "GetError")]
	private static extern ulong GetError();

	[DllImport(Config.LibName, EntryPoint = "GetErrorMessage")]
	private static extern byte[] GetErrorMessage();

	[DllImport(Config.LibName, EntryPoint = "HasError")]
	public static extern bool HasError();

	[DllImport(Config.LibName, EntryPoint = "ClearError")]
	public static extern bool ClearError();

}