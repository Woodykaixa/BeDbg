using System.ComponentModel;
using BeDbg.Api;
using BeDbg.Dto;
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
public abstract partial class BaseDebugger
{
	// Use this CancellationToken to control all threads created by debugger
	protected readonly CancellationTokenSource CancellationTokenSource = new();
	public CancellationToken CancellationToken => CancellationTokenSource.Token;

	public DateTime StartTime { get; } = DateTime.Now;
	public int TargetPid { get; internal set; }

	public long TargetHandle { get; internal set; }

	public List<BeDbg64.ProcessMemoryBlockInformation> MemPages { get; } = new(128);

	public Dictionary<uint, ProcessModel> Processes = new(16);
	public Dictionary<long, RuntimeModuleModel> Modules = new(32);

	private Dictionary<ulong, byte> _breakpointValues = new(64);

	public DebuggingProcess TargetProcess => new()
	{
		AttachTime = StartTime,
		Handle = TargetHandle,
		Id = TargetPid
	};

	public BeDbg64.Registers GetRegisters(ulong thread)
	{
		var process = Processes[(uint) TargetPid];
		var threadHandle = process.Threads[process.MainThread].Handle;
		// if (Kernel.SuspendThread(threadHandle) == uint.MaxValue)
		// {
		// 	throw new Win32Exception();
		// }

		unsafe
		{
			var registers = stackalloc BeDbg64.Registers[1];
			BeDbg64.GetThreadRegisters(threadHandle, registers);
			// if (Kernel.ResumeThread(threadHandle) == uint.MaxValue)
			// {
			// 	throw new Win32Exception();
			// }

			return *registers;
		}
	}


	// protected void ReadProcessModules()
	// {
	// 	// var modules = BeDbg64.QueryProcessModules(new IntPtr(TargetHandle));
	// 	// Modules.Clear();
	// 	// Modules.AddRange(modules.Select(m => new RuntimeModuleModel()));
	// }
	//
	// protected void ReadProcessMemoryPages()
	// {
	// 	MemPages.Clear();
	// 	var pages = BeDbg64.QueryProcessMemoryPages(new IntPtr(TargetHandle));
	// 	MemPages.AddRange(pages);
	// }

	public bool HasBreakpoint(ulong address) => _breakpointValues.ContainsKey(address);

	public IEnumerable<ulong> ListBreakpoints() => _breakpointValues.Keys;

	public void SetBreakpoint(ulong address)
	{
		if (HasBreakpoint(address))
		{
			return;
		}

		if (!BeDbg64.SetBreakpoint(new IntPtr(TargetHandle), address, out var originalCode))
		{
			throw ApiError.FormatError();
		}

		_breakpointValues[address] = originalCode;
	}

	public void RemoveBreakpoint(ulong address)
	{
		if (!HasBreakpoint(address))
		{
			return;
		}

		if (!BeDbg64.RemoveBreakpoint(new IntPtr(TargetHandle), address, _breakpointValues[address]))
		{
			throw ApiError.FormatError();
		}

		_breakpointValues.Remove(address);
	}

	public void StepIn(int threadId)
	{
		var process = Processes[(uint) TargetPid];
		var thread = process.Threads[(uint) threadId];
		var flag = BeDbg64.GetThreadContextFlag(thread.Handle);
		BeDbg64.SetThreadContextFlag(thread.Handle, flag | 0x100);
		Continue(threadId);
	}

	public void Continue(int threadId)
	{
		_continueData.ContinueThread = threadId;
		_continueData.ContinueEvent.Set();
	}

	public IEnumerable<InstructionModel> Disassemble(ulong address, uint size)
	{
		var buffer = new byte[size];

		Kernel.ReadProcessMemory(new IntPtr(TargetHandle), new IntPtr((long) address), buffer, size, out var read);
		var decoder = Decoder.Create(64, new ByteArrayCodeReader(buffer));
		decoder.IP = (ulong) address;
		var endRip = decoder.IP + (uint) read;
		var instructions = new List<Instruction>();
		while (decoder.IP < endRip)
			instructions.Add(decoder.Decode());

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
		return instructions.Where(instr => !instr.IsInvalid).Select(instr =>
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