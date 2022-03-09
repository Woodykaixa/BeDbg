using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using BeDbg.Models;

namespace BeDbg.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class ProcessController : ControllerBase
	{
		[HttpGet]
		public IEnumerable<ProcessModel> Get()
		{
			return Process.GetProcesses()
				.Select(process =>
					new ProcessModel(process.ProcessName, process.Id, process.MainWindowTitle));
		}
	}
}