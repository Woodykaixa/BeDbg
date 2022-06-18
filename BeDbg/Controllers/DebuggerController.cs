using BeDbg.Debugger;
using BeDbg.Dto;
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
		var sender = new ServerEventSender();
		await sender.InitEventAsync(Response);

		var readyEmitEvent = new ManualResetEventSlim(false);
		var lockThread = false;
		var debugger = _debugService.FindOneByIndex(index);
		if (debugger == null)
		{
			await sender.SendEventAsync(new DebuggerEvent
			{
				Event = "notFound",
				Payload = $"Cannot find debugger at index {index}"
			});
			return;
		}

		EventHandler<EmitDebuggerEventArgs> onEmitDebuggerEventHandler =
			(object? o, Debugger.EmitDebuggerEventArgs e) =>
			{
				if (!lockThread)
				{
					return;
				}

				lockThread = false;
				readyEmitEvent.Set();
			};

		debugger.EmitDebuggerEventHandler += onEmitDebuggerEventHandler;

		var eventCount = 0;
		while (true)
		{
			if (lockThread)
			{
				readyEmitEvent.Wait();
			}

			var stopCount = debugger.DebuggerEventList.Count;

			if (eventCount == stopCount)
			{
				lockThread = true;
				readyEmitEvent.Reset();
				continue;
			}

			while (eventCount < stopCount)
			{
				DebuggerEvent dbgEvent;
				lock (debugger.DebuggerEventListLock)
				{
					dbgEvent = debugger.DebuggerEventList[eventCount];
				}

				await sender.SendEventAsync(dbgEvent);
				eventCount++;
				if (dbgEvent.Event != "exitProgram")
				{
					continue;
				}

				debugger.EmitDebuggerEventHandler -= onEmitDebuggerEventHandler;
				break;
			}
		}
	}
}