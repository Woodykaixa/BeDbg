using BeDbg.Api;
using BeDbg.Contexts;
using BeDbg.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeDbg.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class DebuggingProcessController : ControllerBase
	{
		private readonly DebuggingProcessContext _ctx;

		public DebuggingProcessController(DebuggingProcessContext ctx)
		{
			_ctx = ctx;
		}

		[HttpGet]
		public IEnumerable<DebuggingProcess> ListDebuggingProcess()
		{
			return _ctx.DebuggingProcesses.ToList();
		}

		[HttpGet("{pid}")]
		public async Task<ActionResult<DebuggingProcess>> GetProcess(int pid)
		{
			var process = await _ctx.DebuggingProcesses.FindAsync(pid);

			return process != null ? process : NotFound();
		}

		[HttpGet("attach")]
		public async Task<ActionResult<DebuggingProcess>> AttachProcess([FromQuery] int pid)
		{
			var handle = BeDbg64.AttachProcess(pid);
			var process = new DebuggingProcess
			{
				Id = pid,
				AttachTime = DateTime.Now,
				Handle = handle
			};
			_ctx.DebuggingProcesses.Add(process);

			await _ctx.SaveChangesAsync();
			return CreatedAtAction(nameof(GetProcess), new {pid}, process);
		}
	}
}