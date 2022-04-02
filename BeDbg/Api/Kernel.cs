using System.Runtime.InteropServices;

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
	}
}