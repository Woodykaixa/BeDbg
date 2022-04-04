using System.Text.Json;
using BeDbg.Api;
using BeDbg.Debugger;
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
		public ActionResult<DebuggingProcess> GetProcess(int pid)
		{
			var debugger = _debugService.FindOneByPid(pid);
			return debugger != null ? debugger.TargetProcess : NotFound();
		}

		[HttpGet("attach")]
		public ActionResult<DebuggingProcess> AttachProcess([FromQuery] int pid)
		{
			var debugger = _debugService.AttachProcess(pid);
			return debugger;
		}

		[HttpPost("detach")]
		public ActionResult DetachProcess([FromBody] int pid)
		{
			_debugService.StopDebugProcessByPid(pid);
			return Ok();
		}

		[HttpPost]
		public int CreateDebugProcess([FromBody] CreateProcessRequest request)
		{
			var (file, command) = request;
			var process = _debugService.CreateDebugProcess(file, command, null, null);

			return process.Id;
		}

		[HttpGet("modules")]
		public ActionResult<IEnumerable<ProcessModule>> DumpProcessModules([FromQuery] int pid)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger != null)
			{
				return Ok(debugger.Modules.Select(mod => new ProcessModule(mod.Name, mod.Entry, mod.Size, mod.Size)));
			}

			return NotFound();
		}

		[HttpGet("pages")]
		public ActionResult<IEnumerable<ProcessMemPage>> DumpProcessMemoryPages([FromQuery] int pid)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}

			return Ok(debugger.MemPages.Select(info => new ProcessMemPage(info.BaseAddress, info.AllocAddress,
				info.Size,
				ProcessModelHelper.MemoryProtectionFromFlag(info.ProtectionFlags),
				ProcessModelHelper.MemoryProtectionFromFlag(info.InitialProtectionFlags), info.State,
				info.Type)));
		}

		[HttpGet("disasm")]
		public ActionResult Disassemble([FromQuery] int pid)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}

			return Ok(debugger.Disassemble(0x00007FF79E3C1000, 0x3000));
		}
	}
}