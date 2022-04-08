using System.Runtime.InteropServices;
using BeDbg.Api;

namespace BeDbg.Debugger;

public abstract class DebugEventHandler
{
	public abstract unsafe bool OnException(uint process, uint thread, void* info);
	public abstract unsafe bool OnCreateThread(uint process, uint thread, void* info);
	public abstract unsafe bool OnCreateProcess(uint process, uint thread, void* info);
	public abstract unsafe bool OnExitThread(uint process, uint thread, void* info);
	public abstract unsafe bool OnExitProcess(uint process, uint thread, void* info);
	public abstract unsafe bool OnLoadDll(uint process, uint thread, void* info);
	public abstract unsafe bool OnUnloadDll(uint process, uint thread, void* info);
	public abstract unsafe bool OnOutputDebugString(uint process, uint thread, void* info);
	public abstract unsafe bool OnRip(uint process, uint thread, void* info);

	public unsafe delegate bool DebugEventCallback(uint process, uint thread, void* info);

	protected IntPtr CallbackHandle;

	protected DebugEventHandler()
	{
		CallbackHandle = CreateDebugLoopCallbacks();
		unsafe
		{
			SetDebugLoopCallback(CallbackHandle, 1, OnException);
			SetDebugLoopCallback(CallbackHandle, 2, OnCreateThread);
			SetDebugLoopCallback(CallbackHandle, 3, OnCreateProcess);
			SetDebugLoopCallback(CallbackHandle, 4, OnExitThread);
			SetDebugLoopCallback(CallbackHandle, 5, OnExitProcess);
			SetDebugLoopCallback(CallbackHandle, 6, OnLoadDll);
			SetDebugLoopCallback(CallbackHandle, 7, OnUnloadDll);
			SetDebugLoopCallback(CallbackHandle, 8, OnOutputDebugString);
			SetDebugLoopCallback(CallbackHandle, 9, OnRip);
		}
	}

	~DebugEventHandler()
	{
		DestroyDebugLoopCallbacks(CallbackHandle);
	}

	[DllImport(InteropConfig.Api64, EntryPoint = "CreateDebugLoopCallbacks")]
	protected static extern IntPtr CreateDebugLoopCallbacks();

	[DllImport(InteropConfig.Api64, EntryPoint = "SetDebugLoopCallback")]
	protected static extern void SetDebugLoopCallback(IntPtr callbacks, int eventId,
		[MarshalAs(UnmanagedType.FunctionPtr)] DebugEventCallback callback);

	[DllImport(InteropConfig.Api64, EntryPoint = "DestroyDebugLoopCallbacks")]
	protected static extern void DestroyDebugLoopCallbacks(IntPtr callbacks);

	[DllImport(InteropConfig.Api64, EntryPoint = "DebugLoopWaitEvent")]
	protected static extern bool DebugLoopWaitEvent(IntPtr callbacks);
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct ExceptionRecord
{
	public uint ExceptionCode;
	public uint ExceptionFlags;
	public IntPtr NextException; // ExceptionRecord
	public IntPtr ExceptionAddress;
	public uint NumberParameters;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15, ArraySubType = UnmanagedType.SysInt)]
	public ulong[] ExceptionInformation;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct ExceptionDebugInfo
{
	public ExceptionRecord ExceptionRecord;
	public uint dwFirstChance;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct CreateThreadDebugInfo
{
	public IntPtr hThread;
	public IntPtr lpThreadLocalBase;
	public IntPtr lpStartAddress;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct CreateProcessDebugInfo
{
	public IntPtr hFile;
	public IntPtr hProcess;
	public IntPtr hThread;
	public IntPtr lpBaseOfImage;
	public uint dwDebugInfoFileOffset;
	public uint nDebugInfoSize;
	public IntPtr lpThreadLocalBase;
	public IntPtr lpStartAddress;
	public IntPtr lpImageName;
	public ushort fUnicode;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct ExitProcessDebugInfo
{
	public uint dwExitCode;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct ExitThreadDebugInfo
{
	public uint dwExitCode;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct LoadDllDebugInfo
{
	public IntPtr hFile;
	public IntPtr lpBaseOfDll;
	public uint dwDebugInfoFileOffset;
	public uint nDebugInfoSize;
	public IntPtr lpImageName;
	public ushort fUnicode;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct UnloadDllDebugInfo
{
	public IntPtr lpBaseOfDll;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct OutputDebugStringInfo
{
	[MarshalAs(UnmanagedType.LPStr)] public IntPtr lpDebugStringData;
	public ushort fUnicode;
	public ushort nDebugStringLength;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct RipInfo
{
	public uint dwError;
	public uint dwType;
}