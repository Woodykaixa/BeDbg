using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Management;
using BeDbg.Extensions;
using BeDbg.Models;

namespace BeDbg.Controllers;

[Route("[controller]")]
[ApiController]
public class ProcessController : ControllerBase
{
	[HttpGet]
	public IEnumerable<ProcessModel> GetProcesses()
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

		var getCommand = (int pid) =>
		{
			if (!cmdDict.ContainsKey(pid))
			{
				using var processSearcher = new ManagementObjectSearcher(
					$"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {pid}");
				using var objects = processSearcher.Get();
				var @object = objects.Cast<ManagementBaseObject>().SingleOrDefault();
				var cmd = @object?["CommandLine"]?.ToString() ?? string.Empty;
				cmdDict[pid] = cmd;
			}

			return cmdDict[pid];
		};

		return Process.GetProcesses().Where(process => !process.Invisible())
			.Select(process =>
				new ProcessModel(process.ProcessName, process.Id, process.MainWindowTitle, process.IsWow64(),
					getCommand(process.Id)));
	}

	[HttpPost]
	public ProcessModel CreateProcess([FromBody] CreateProcessRequest request)
	{
		var process = new Process();
		var startup = new ProcessStartInfo(request.File, request.Command);
		process.StartInfo = startup;
		process.Start();
		return new ProcessModel(process.ProcessName, process.Id, process.MainWindowTitle, process.IsWow64(),
			$"{request.File} {request.Command}");
	}
}