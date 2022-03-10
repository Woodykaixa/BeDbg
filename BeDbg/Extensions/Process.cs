using System.ComponentModel;
using System.Management;
using System.Diagnostics;

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
}