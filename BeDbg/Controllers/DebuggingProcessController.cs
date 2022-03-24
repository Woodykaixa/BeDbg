using System.Runtime.InteropServices;
using BeDbg.Api;
using BeDbg.Contexts;
using BeDbg.Models;
using BeDbg.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeDbg.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class DebuggingProcessController : ControllerBase
	{
		private readonly DebugService _debugService;

		public DebuggingProcessController(DebugService debugService)
		{
			_debugService = debugService;
		}

		[HttpGet]
		public IEnumerable<DebuggingProcess> ListDebuggingProcess() => _debugService.ListDebuggingProcesses();

		[HttpGet("{pid:int}")]
		public async Task<ActionResult<DebuggingProcess>> GetProcess(int pid)
		{
			var process = await _debugService.FindOneByPid(pid);
			return process != null ? process : NotFound();
		}

		[HttpGet("attach")]
		public async Task<ActionResult<DebuggingProcess>> AttachProcess([FromQuery] int pid)
		{
			var process = await _debugService.AttachProcess(pid);
			return CreatedAtAction(nameof(GetProcess), new {pid}, process);
		}

		[HttpPost("detach")]
		public async Task<ActionResult> DetachProcess([FromBody] int pid)
		{
			await _debugService.DetachProcess(pid);
			return Ok();
		}

		[HttpPost]
		public async Task<int> CreateDebugProcess([FromBody] CreateProcessRequest request)
		{
			var (file, command) = request;
			var pid = BeDbg64.StartProcess(file, command, null, null);
			if (pid is 0)
			{
				return 0;
			}

			var process = await _debugService.AttachProcess(pid);
			return process.Id;
		}

		[HttpGet("modules")]
		public async Task<ActionResult<IEnumerable<ProcessModule>>> DumpProcessModules([FromQuery] int pid)
		{
			var process = await _debugService.FindOneByPid(pid);
			if (process != null)
			{
				return Ok(BeDbg64.QueryProcessModules(new IntPtr(process.Handle)));
			}

			return NotFound();
		}
	}
}