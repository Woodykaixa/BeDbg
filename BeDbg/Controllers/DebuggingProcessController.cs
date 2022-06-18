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
				return Ok(debugger.Modules.Select(mod =>
					new ProcessModule(mod.Value.ImageName, (ulong) mod.Value.ImageBase.ToInt64(),
						0,
						(ulong) mod.Value.ImageBase.ToInt64())));
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

		[HttpGet("disasm/{pid:int}")]
		public ActionResult Disassemble(int pid, [FromQuery] ulong address)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}


			return Ok(debugger.Disassemble(address, 0x3000));
		}

		[HttpGet("regs/{pid:int}")]
		public ActionResult GetRegisters(int pid, [FromQuery] ulong tid)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}


			return Ok(debugger.GetRegisters(tid));
		}

		[HttpPost("step_in/{pid:int}")]
		public ActionResult StepIn(int pid, [FromQuery] int tid)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}

			debugger.StepIn(tid);
			return Ok();
		}

		[HttpPost("continue/{pid:int}")]
		public ActionResult Continue(int pid, [FromQuery] int tid)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}

			debugger.Continue(tid);
			return Ok();
		}

		[HttpGet("bp/{pid:int}/{address:long}")]
		public ActionResult<bool> HasBreakpoint(int pid, long address)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}

			return Ok(debugger.HasBreakpoint((ulong) address));
		}

		[HttpGet("bp/{pid:int}")]
		public ActionResult<IEnumerable<ulong>> ListBreakpoint(int pid)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}

			return Ok(debugger.ListBreakpoints());
		}

		[HttpPost("bp/{pid:int}")]
		public ActionResult SetBreakpoint(int pid, [FromBody] ulong address)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return NotFound();
			}

			debugger.SetBreakpoint(address);
			return Ok();
		}

		[HttpDelete("bp/{pid:int}")]
		public ActionResult RemoveBreakpoint(int pid, [FromBody] long address)
		{
			var debugger = _debugService.FindOneByPid(pid);
			if (debugger == null)
			{
				return BadRequest();
			}

			debugger.RemoveBreakpoint((ulong) address);
			return Ok();
		}
	}
}