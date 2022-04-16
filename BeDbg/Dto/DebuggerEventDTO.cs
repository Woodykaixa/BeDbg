namespace BeDbg.Dto;

public class DebuggerEvent<T> where T : DebuggerEventPayload
{
	public T? Payload { get; set; }
	public string Event { get; set; } = "message";
	public string? Id { get; set; }
}

public abstract class DebuggerEventPayload
{
	public uint Process { get; set; }
	public uint Thread { get; set; }
}

public class ExceptionPayload : DebuggerEventPayload
{
	public uint ExceptionCode { get; set; }
	public uint ExceptionFlag { get; set; }
	public long ExceptionAddress { get; set; }
	public uint FirstChance { get; set; }
}

public class CreateThreadPayload : DebuggerEventPayload
{
	public long ThreadLocalBase { get; set; }
	public long StartAddress { get; set; }
}

public class CreateProcessPayload : DebuggerEventPayload
{
	public long ThreadLocalBase { get; set; }
	public long StartAddress { get; set; }
	public long BaseOfImage { get; set; }
}

public class ExitThreadPayload : DebuggerEventPayload
{
	public uint ExitCode { get; set; }
}

public class ExitProcessPayload : ExitThreadPayload
{
}

public class LoadDllPayload : DebuggerEventPayload
{
	public long BaseOfDll { get; set; }
}

public class UnloadDllPayload : DebuggerEventPayload
{
	public long BaseOfDll { get; set; }
}

public class OutputDebugStringPayload : DebuggerEventPayload
{
	// TODO: handle OutputDebugString Event
}

public class RipPayload : DebuggerEventPayload
{
	public uint Error { get; set; }
	public uint Type { get; set; }
}