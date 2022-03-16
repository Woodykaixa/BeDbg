using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Management;
using BeDbg.Api;
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
			"SELECT Name, CommandLine, ProcessId FROM Win32_Process");
		using var objects = searcher.Get();
		var cmdDict = new Dictionary<int, ProcessModel>(objects.Count);
		foreach (var obj in objects)
		{
			try
			{
				var cmd = obj["CommandLine"]?.ToString() ?? string.Empty;
				var name = obj["Name"]!.ToString() ?? string.Empty;
				var pid = int.Parse(obj["ProcessId"]?.ToString() ?? "0");
				Kernel.IsWow64Process(pid, out var wow64);

				if (pid is 0 or 4)
				{
					continue;
				}

				if (BeDbg64.IsAttachableProcess(pid))
				{
					cmdDict[pid] = new ProcessModel(name, pid, "", wow64, cmd);
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
			}
		}

		return cmdDict.Values;
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


	[HttpGet("read")]
	public FileContentResult ReadProcessMemory([FromQuery] ReadProcessMemoryRequest request)
	{
		var span = new byte[10];
		span[0] = 0x01;
		span[1] = 0x02;
		span[2] = 0x03;
		span[3] = 0x04;
		span[4] = 0x05;
		span[5] = 0x06;
		span[6] = 0x07;
		span[7] = 0x08;
		span[8] = 0x09;
		span[9] = 0x0A;
		return new FileContentResult(span, "application/octet-stream");
	}
}