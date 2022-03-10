using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Management;
using BeDbg.Extensions;
using BeDbg.Models;

namespace BeDbg.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class ProcessController : ControllerBase
	{
		[HttpGet]
		public IEnumerable<ProcessModel> Get()
		{
			using var searcher = new ManagementObjectSearcher(
				"SELECT CommandLine, ProcessId FROM Win32_Process");
			using var objects = searcher.Get();
			var cmdDict = new Dictionary<int, string>(objects.Count);
			foreach (var obj in objects)
			{
				try
				{
					var cmd = obj["CommandLine"]?.ToString() ?? string.Empty;
					var pid = int.Parse(obj["ProcessId"]?.ToString() ?? "0");
					if (pid != 0)
					{
						cmdDict[pid] = cmd;
					}
				}
				catch
				{
					// ignored
				}
			}

			return Process.GetProcesses().Where(process => !process.Invisible())
				.Select(process =>
					new ProcessModel(process.ProcessName, process.Id, process.MainWindowTitle, false,
						cmdDict[process.Id]));
		}
	}
}