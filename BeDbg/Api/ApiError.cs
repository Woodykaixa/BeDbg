using System.Runtime.InteropServices;
using BeDbg.Exceptions;

namespace BeDbg.Api;

public class ApiError
{
	[DllImport(InteropConfig.Api64, EntryPoint = "HasError")]
	public static extern bool HasError();

	[DllImport(InteropConfig.Api64, EntryPoint = "ClearError")]
	public static extern bool Clear();

	[DllImport(InteropConfig.Api64, EntryPoint = "GetError")]
	public static extern ulong GetError();

	[DllImport(InteropConfig.Api64, EntryPoint = "GetErrorMessage", CharSet = CharSet.Unicode)]
	public static extern IntPtr GetErrorMessage();

	public static ApiException FormatError()
	{
		var errorCode = GetError();
		var module = (uint)(errorCode >> 32);
		var code = (uint)errorCode;
		var msg = GetErrorMessage();
		return new ApiException(Marshal.PtrToStringAuto(msg)!, code, module);
	}
}