using BeDbg.Services;
using BeDbg.Util;
using Microsoft.AspNetCore.Mvc;

namespace BeDbg.Controllers;

[Route("[controller]")]
[ApiController]
public class DebuggerController : ControllerBase
{
	private readonly DebugService _debugService;

	public DebuggerController(DebugService debugService)
	{
		_debugService = debugService;
	}

	[HttpGet("{pid:int}/event")]
	public async Task ServerSentDebuggingEvent(int pid)
	{
		var debugger = _debugService.FindOneByPid(pid);
		if (debugger == null)
		{
			return;
		}

		var sender = new ServerEventSender();
		await sender.InitEventAsync(Response);

		while (true)
		{
			if (debugger.DebuggerEventList.Count == 0)
			{
				continue;
			}

			var dbgEvent = debugger.DebuggerEventList.Dequeue();
			await sender.SendEventAsync(dbgEvent);
		}
	}
}