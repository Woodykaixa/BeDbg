using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using BeDbg.Exceptions;

namespace BeDbg.Api;

public record BeDbgApiError(string Module, uint Code, string Message);

public class BeDbg64
{
	[DllImport(InteropConfig.Api64, EntryPoint = "AttachProcess")]
	private static extern IntPtr _attachProcess(int pid);

	[DllImport(InteropConfig.Api64, EntryPoint = "DetachProcess")]
	public static extern bool _detachProcess(IntPtr handle);

	[DllImport(InteropConfig.Api64, EntryPoint = "StartProcess", CharSet = CharSet.Unicode)]
	private static extern int _startProcess(string filename, string command, string? environment,
		string? workingDirectory);

	public static int StartProcess(string filename, string command, string? environment,
		string? workingDirectory)
	{
		var pid = _startProcess(filename, command, environment, workingDirectory);
		if (HasError())
		{
			Throw();
		}

		return pid;
	}

	private static void Throw()
	{
		var errorCode = GetError();
		var module = (uint) (errorCode >> 32);
		var code = (uint) errorCode;
		var msg = Marshal.PtrToStringAuto(GetErrorMessage());

		throw new ApiException(msg ?? "", code, module);
	}

	[DllImport(InteropConfig.Api64, EntryPoint = "HasError")]
	public static extern bool HasError();

	[DllImport(InteropConfig.Api64, EntryPoint = "ClearError")]
	public static extern bool ClearError();

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
		var msg = Marshal.PtrToStringAuto(GetErrorMessage());

		throw new Exception($"Module: {module}, Code: {code}, Msg: {msg}");
	}

	public static bool DetachProcess(IntPtr handle)
	{
		ClearError();
		var success = _detachProcess(handle);
		if (!success)
		{
			Throw();
		}

		return success;
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