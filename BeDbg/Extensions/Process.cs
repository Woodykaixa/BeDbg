using System.ComponentModel;
using System.Management;
using System.Diagnostics;
using BeDbg.Api;

namespace BeDbg.Extensions;

public static class ProcessExtension
{
	public static bool Invisible(this Process process)
	{
		switch (process.Id)
		{
			case 0:
				return true;
			default:
				return false;
		}
	}

	public static bool IsWow64(this Process process)
	{
		Kernel.IsWow64Process(process.Id, out var isWow64);
		return isWow64;
	}
}