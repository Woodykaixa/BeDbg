using System.Buffers;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using BeDbg.Exceptions;
using BeDbg.Models;

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
		Console.WriteLine($"Msg: {msg}, Code: {code}, Module: {module}");
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

	[DllImport(InteropConfig.Api64, EntryPoint = "QueryProcessModules")]
	private static extern unsafe bool _queryProcessModules(IntPtr handle,
		[Out] ProcessModuleInformation[] modules,
		uint count, uint* usedCount);

	[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
	public struct ProcessModuleInformation
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string Name;

		public ulong Entry;
		public uint Size;
		public ulong ImageBase;
	}

	public static IEnumerable<ProcessModule> QueryProcessModules(IntPtr handle)
	{
		uint used = 0;
		var infos = new ProcessModuleInformation[1024];
		unsafe
		{
			var success = _queryProcessModules(handle, infos, 1024, &used);
			if (!success)
			{
				Throw();
			}

			return infos.Take((int) (used))
				.Select(info => new ProcessModule(info.Name, info.Entry, info.Size, info.ImageBase));
		}
	}


	[DllImport(InteropConfig.Api64, EntryPoint = "QueryProcessMemoryInfos")]
	private static extern ulong _queryProcessMemoryInfos(IntPtr handle,
		[Out] ProcessMemoryBlockInformation[] infos, ulong count);

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct ProcessMemoryBlockInformation
	{
		public ulong BaseAddress;
		public ulong AllocAddress;
		public ulong Size;
		public uint ProtectionFlags;
		public uint InitialProtectionFlags;
		public uint State;
		public uint Type;
	}

	public static IEnumerable<ProcessMemPage> QueryProcessMemoryPages(IntPtr handle)
	{
		ClearError();
		var infos = new ProcessMemoryBlockInformation[1024];
		var count = _queryProcessMemoryInfos(handle, infos, 1024);
		return infos.Take((int) count).Select(info => new ProcessMemPage(info.BaseAddress, info.AllocAddress, info.Size,
			ProcessModelHelper.MemoryProtectionFromFlag(info.ProtectionFlags),
			ProcessModelHelper.MemoryProtectionFromFlag(info.InitialProtectionFlags), info.State,
			info.Type));
	}
}