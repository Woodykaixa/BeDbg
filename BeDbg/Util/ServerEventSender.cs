using System.Text.Json;
using BeDbg.Dto;

namespace BeDbg.Util;

public class ServerEventSender
{
	private HttpResponse? Response { get; set; }

	public async Task InitEventAsync(HttpResponse response, int retry = 5000)
	{
		Response = response;

		Response.ContentType = "text/event-stream";
		Response.Headers.CacheControl = "no-cache";

		await Response.WriteAsync($"retry: {retry}\n");
		await Response.Body.FlushAsync();
	}

	public async Task SendEventAsync<T>(DebuggerEvent<T> debuggerEvent) where T : DebuggerEventPayload
	{
		if (Response == null)
		{
			throw new Exception("Set Response via InitEventAsync");
		}

		if (debuggerEvent.Id != null)
		{
			await Response.WriteAsync($"id: {debuggerEvent.Id}\n");
		}

		if (debuggerEvent.Event != "message")
		{
			await Response.WriteAsync($"event: {debuggerEvent.Event}\n");
		}

		// Write: data: {data}\n\n
		await Response.WriteAsync("data: ");
		var jsonData = JsonSerializer.Serialize((object) debuggerEvent.Payload!, new JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});
		await Response.WriteAsync(jsonData);
		await Response.WriteAsync("\n\n");
	}
}