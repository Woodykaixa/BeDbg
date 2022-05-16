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

	[DllImport(InteropConfig.Api64, EntryPoint = "CopyProcessHandle")]
	public static extern IntPtr CopyProcessHandle(IntPtr handle);

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct XmmRegister
	{
		public ulong Low { get; set; }
		public ulong High { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct FpuRegister
	{
		public double Low { get; set; }
		public ulong High { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct Registers
	{
		public ulong Rip { get; set; }
		public ulong Rax { get; set; }
		public ulong Rcx { get; set; }
		public ulong Rdx { get; set; }
		public ulong Rbx { get; set; }
		public ulong Rsp { get; set; }
		public ulong Rbp { get; set; }
		public ulong Rsi { get; set; }
		public ulong Rdi { get; set; }
		public ulong R8 { get; set; }
		public ulong R9 { get; set; }
		public ulong R10 { get; set; }
		public ulong R11 { get; set; }
		public ulong R12 { get; set; }
		public ulong R13 { get; set; }
		public ulong R14 { get; set; }
		public ulong R15 { get; set; }

		public uint MxCsr { get; set; }

		public ushort SegCs { get; set; }
		public ushort SegDs { get; set; }
		public ushort SegEs { get; set; }
		public ushort SegFs { get; set; }
		public ushort SegGs { get; set; }
		public ushort SegSs { get; set; }
		public uint EFlags { get; set; }

		public ulong Dr0 { get; set; }
		public ulong Dr1 { get; set; }
		public ulong Dr2 { get; set; }
		public ulong Dr3 { get; set; }
		public ulong Dr6 { get; set; }
		public ulong Dr7 { get; set; }

		public byte FpuTagWord { get; set; }
		public ushort FpuStatusWord { get; set; }
		public ushort FpuControlWord { get; set; }

		public FpuRegister St0 { get; set; }
		public FpuRegister St1 { get; set; }
		public FpuRegister St2 { get; set; }
		public FpuRegister St3 { get; set; }
		public FpuRegister St4 { get; set; }
		public FpuRegister St5 { get; set; }
		public FpuRegister St6 { get; set; }
		public FpuRegister St7 { get; set; }

		public XmmRegister Xmm0 { get; set; }
		public XmmRegister Xmm1 { get; set; }
		public XmmRegister Xmm2 { get; set; }
		public XmmRegister Xmm3 { get; set; }
		public XmmRegister Xmm4 { get; set; }
		public XmmRegister Xmm5 { get; set; }
		public XmmRegister Xmm6 { get; set; }
		public XmmRegister Xmm7 { get; set; }
		public XmmRegister Xmm8 { get; set; }
		public XmmRegister Xmm9 { get; set; }
		public XmmRegister Xmm10 { get; set; }
		public XmmRegister Xmm11 { get; set; }
		public XmmRegister Xmm12 { get; set; }
		public XmmRegister Xmm13 { get; set; }
		public XmmRegister Xmm14 { get; set; }
		public XmmRegister Xmm15 { get; set; }
	}

	[DllImport(InteropConfig.Api64, EntryPoint = "GetThreadRegisters")]
	public static extern unsafe void GetThreadRegisters(IntPtr thread,
		Registers* reg);

	[DllImport(InteropConfig.Api64, EntryPoint = "GetThreadContextFlag")]
	public static extern uint GetThreadContextFlag(IntPtr threadHandle);

	[DllImport(InteropConfig.Api64, EntryPoint = "SetThreadContextFlag")]
	public static extern bool SetThreadContextFlag(IntPtr threadHandle, uint flag);


	[DllImport(InteropConfig.Api64)]
	public static extern bool SetBreakpoint(IntPtr processHandle, ulong address,
		[Out] out byte originalCode);

	[DllImport(InteropConfig.Api64)]
	public static extern bool RemoveBreakpoint(IntPtr processHandle, ulong address, byte originalCode);
}