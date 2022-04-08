using System.Runtime.InteropServices;
using BeDbg.Debugger;

namespace BeDbg.Api
{
	public class Kernel
	{
		[DllImport(InteropConfig.Kernel, EntryPoint = "K32EnumProcesses")]
		public static extern unsafe bool EnumProcesses(int* processSpan, uint size, out uint sizeRead);

		[DllImport(InteropConfig.Kernel, EntryPoint = "IsWow64Process")]
		public static extern bool IsWow64Process(int pid, out bool isWow64);

		[DllImport(InteropConfig.Kernel, EntryPoint = "ReadProcessMemory")]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
			uint nSize, [Out] out uint lpNumberOfBytesRead);

		[DllImport(InteropConfig.Kernel, EntryPoint = "TerminateProcess")]
		public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

		[DllImport(InteropConfig.Kernel, EntryPoint = "DebugActiveProcess")]
		public static extern bool DebugActiveProcess(int pid);

		[DllImport(InteropConfig.Kernel, EntryPoint = "DebugActiveProcessStop")]
		public static extern bool DebugActiveProcessStop(int pid);

		[DllImport(InteropConfig.Kernel, EntryPoint = "CloseHandle")]
		public static extern bool CloseHandle(IntPtr handle);

		public unsafe delegate void ThreadCb(void* param);

		[DllImport(InteropConfig.Kernel, EntryPoint = "CreateThread")]
		public static extern bool CreateThread(string? fake, int stack, ThreadCb cb);

		[DllImport(InteropConfig.Kernel, EntryPoint = "ResumeThread")]
		public static extern uint ResumeThread(IntPtr thread);
	}
}