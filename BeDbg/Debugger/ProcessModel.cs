using System.Runtime.InteropServices;
using BeDbg.Api;

namespace BeDbg.Debugger;

/// <summary>
/// <see cref="BaseDebugger"/> uses this class to interact with Windows Process
/// </summary>
public class ProcessModel
{
	[DllImport(InteropConfig.Kernel, EntryPoint = "ReadProcessMemory")]
	private static extern unsafe bool ReadProcessMemory(
		IntPtr process,
		IntPtr address,
		[Out] byte* buffer,
		int size,
		[Out] int* numberOfBytesRead
	);

	public uint Id { get; set; }
	public uint MainThread { get; set; }
	public Dictionary<uint, ThreadModel> Threads = new(16);
	public IntPtr Handle { get; set; }
	public RuntimeModuleModel Executable { get; set; }

	public Span<byte> ReadMemory(IntPtr address, int size)
	{
		var buffer = new byte[size];
		unsafe
		{
			fixed (byte* ptr = buffer)
			{
				int read;
				ReadProcessMemory(Handle, address, ptr, size, &read);
				if (read != size)
				{
					throw new Exception("Cannot read process memory");
				}

				return buffer;
			}
		}
	}
}