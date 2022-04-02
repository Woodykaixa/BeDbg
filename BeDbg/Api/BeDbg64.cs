using System.Runtime.InteropServices;
using BeDbg.Debugger;
using BeDbg.Models;

namespace BeDbg.Api;

public class BeDbg64
{
	[DllImport(InteropConfig.Api64, EntryPoint = "AttachProcess")]
	public static extern IntPtr AttachProcess(int pid);

	[DllImport(InteropConfig.Api64, EntryPoint = "DetachProcess")]
	public static extern bool DetachProcess(IntPtr handle);


	[DllImport(InteropConfig.Api64, EntryPoint = "IsAttachableProcess")]
	public static extern bool IsAttachableProcess(int pid);


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

	public static IEnumerable<ProcessModuleInformation> QueryProcessModules(IntPtr handle)
	{
		uint used = 0;
		var infos = new ProcessModuleInformation[1024];
		unsafe
		{
			var success = _queryProcessModules(handle, infos, 1024, &used);
			if (!success)
			{
				throw ApiError.FormatError();
			}

			return infos.Take((int) used);
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

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string Info;
	}

	public static IEnumerable<ProcessMemoryBlockInformation> QueryProcessMemoryPages(IntPtr handle)
	{
		ApiError.Clear();
		var infos = new ProcessMemoryBlockInformation[1024];
		var count = _queryProcessMemoryInfos(handle, infos, 1024);
		return infos.Take((int) count);
	}
}