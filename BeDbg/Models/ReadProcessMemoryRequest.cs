namespace BeDbg.Models;

public record ReadProcessMemoryRequest(int Pid, UIntPtr Address, uint Size);