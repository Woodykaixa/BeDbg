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

	[HttpGet("{index:int}/event")]
	public async Task ServerSentDebuggingEvent(int index)
	{
		var debugger = _debugService.FindOneByIndex(index);
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