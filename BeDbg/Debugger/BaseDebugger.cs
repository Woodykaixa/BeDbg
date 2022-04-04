using BeDbg.Api;
using BeDbg.Models;
using Iced.Intel;

namespace BeDbg.Debugger;

/// <summary>
/// <para>
/// Base debugger type defines common operations and properties for debugging. Inherit this class
/// to implement a debugger class and hold it in memory database, all network request works on it.
/// </para>
///
/// <para>
/// If you inherit this class, make sure to initialize debugger resources in constructor and release
/// in destructor. This behavior looks like RAII in C++.
/// </para>
///
/// <para>
/// Classes inherited this class: <see cref="CreateDebugger"/>, <see cref="AttachDebugger"/>
/// </para>
/// </summary>
public abstract class BaseDebugger
{
	public DateTime StartTime { get; } = DateTime.Now;
	public int TargetPid { get; internal set; }
	public long TargetHandle { get; internal set; }
	public List<BeDbg64.ProcessModuleInformation> Modules { get; } = new(128);
	public List<BeDbg64.ProcessMemoryBlockInformation> MemPages { get; } = new(128);

	private ulong _rip = 0;

	protected BaseDebugger(int pid, long targetHandle)
	{
		TargetPid = pid;
		TargetHandle = targetHandle;
	}

	public DebuggingProcess TargetProcess => new()
	{
		AttachTime = StartTime,
		Handle = TargetHandle,
		Id = TargetPid
	};

	protected void ReadProcessModules()
	{
		var modules = BeDbg64.QueryProcessModules(new IntPtr(TargetHandle));
		Modules.Clear();
		Modules.AddRange(modules);
	}

	protected void ReadProcessMemoryPages()
	{
		MemPages.Clear();
		var pages = BeDbg64.QueryProcessMemoryPages(new IntPtr(TargetHandle));
		MemPages.AddRange(pages);
	}

	public IEnumerable<InstructionModel> Disassemble(ulong address, uint size)
	{
		var buffer = new byte[size];
		Kernel.ReadProcessMemory(new IntPtr(TargetHandle), new IntPtr((long)address), buffer, size, out var read);
		var decoder = Decoder.Create(64, new ByteArrayCodeReader(buffer));
		decoder.IP = (ulong)address;
		var endRip = decoder.IP + (uint) read;
		var instructions = new List<Instruction>();
		while (decoder.IP < endRip)
			instructions.Add(decoder.Decode());
		_rip = decoder.IP;

		var formatter = new NasmFormatter()
		{
			Options =
			{
				DigitSeparator = null,
				FirstOperandCharIndex = 0,
				AddLeadingZeroToHexNumbers = false,
				AlwaysShowSegmentRegister = true,
				NasmShowSignExtendedImmediateSize = true
			}
		};
		var output = new StringOutput();
		return instructions.Where(instr => instr.IsInvalid).Select(instr =>
		{
			formatter.Format(instr, output);
			return new InstructionModel()
			{
				Ip = instr.IP,
				Text = output.ToStringAndReset()
			};
		});
	}
}