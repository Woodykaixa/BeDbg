using System.Runtime.InteropServices;
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
			BeDbg64.ClearError();
			var handle = BeDbg64.AttachProcess(pid);
			var process = new DebuggingProcess
			{
				Id = pid,
				AttachTime = DateTime.Now,
				Handle = handle.ToInt64()
			};
			_ctx.DebuggingProcesses.Add(process);

			await _ctx.SaveChangesAsync();
			return CreatedAtAction(nameof(GetProcess), new {pid}, process);
		}

		[HttpPost("detach")]
		public async Task<ActionResult> DetachProcess([FromBody] int pid)
		{
			BeDbg64.ClearError();
			var process = await _ctx.DebuggingProcesses.FindAsync(pid);
			if (process == null)
			{
				return Ok();
			}

			if (BeDbg64.DetachProcess(new IntPtr(process.Handle)))
			{
				_ctx.DebuggingProcesses.Remove(process);
				await _ctx.SaveChangesAsync();

				return Ok();
			}

			var errorCode = BeDbg64.GetError();
			var module = (uint) (errorCode >> 32);
			var code = (uint) errorCode;
			var msg = Marshal.PtrToStringAuto(BeDbg64.GetErrorMessage());

			throw new Exception($"Module: {module}, Code: {code}, Msg: {msg}");
		}
	}
}