namespace BeDbg.Models;

public record MemoryProtection
(
	bool Execute,
	bool Read,
	bool Write,
	bool Copy,
	bool Guard
);

public record ProcessMemPage(
	ulong BaseAddress,
	ulong AllocAddress,
	ulong Size,
	MemoryProtection Flags,
	MemoryProtection InitialFlags,
	uint State,
	uint Type
);

public static class ProcessModelHelper
{
	public static MemoryProtection MemoryProtectionFromFlag(uint flag)
	{
		// see https://docs.microsoft.com/en-us/windows/win32/memory/memory-protection-constants#constants
		var protect = (flag & 0xff);
		var execute = protect >= 0x10;
		var restProtect = (protect & 0x0f) > 0 ? protect & 0x0f : protect >> 4;
		var read = restProtect >= 0x02;
		var write = restProtect >= 0x04;
		var copy = restProtect == 0x08;
		var guard = (flag & 0x100) == 0x100;
		return new MemoryProtection(execute, read, write, copy, guard);
	}
}