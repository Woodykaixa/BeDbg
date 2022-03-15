using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace BeDbg.Api;

public record BeDbgApiError(string Module, uint Code, string Message);

public class BeDbg64
{
	[DllImport(InteropConfig.Api64, EntryPoint = "AttachProcess")]
	private static extern IntPtr _attachProcess(int pid);

	[DllImport(InteropConfig.Api64, EntryPoint = "DetachProcess")]
	public static extern IntPtr DetachProcess(int pid);

	[DllImport(InteropConfig.Api64, EntryPoint = "HasError")]
	public static extern bool HasError();

	[DllImport(InteropConfig.Api64, EntryPoint = "GetError")]
	public static extern ulong GetError();

	[DllImport(InteropConfig.Api64, EntryPoint = "GetErrorMessage", CharSet = CharSet.Unicode)]
	public static extern IntPtr GetErrorMessage();

	public static IntPtr AttachProcess(int pid)
	{
		var handle = _attachProcess(pid);
		if (!HasError())
		{
			return handle;
		}

		var errorCode = GetError();
		var module = (uint) (errorCode >> 32);
		var code = (uint) errorCode;
		throw new Exception($"Module: {module}, Code: {code}");
	}

	[DllImport(InteropConfig.Api64, EntryPoint = "IsAttachableProcess")]
	public static extern bool IsAttachableProcess(int pid);

	public static BeDbgApiError FormatError()
	{
		var errorCode = GetError();
		var module = (uint) (errorCode >> 32);
		var code = (uint) errorCode;
		var msg = GetErrorMessage();
		return new BeDbgApiError(module.ToString(), code, Marshal.PtrToStringAuto(msg)!);
	}
}