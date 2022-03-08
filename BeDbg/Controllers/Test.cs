using Microsoft.AspNetCore.Mvc;

namespace BeDbg.Controllers;

public class Test : Controller
{
	// GET
	[HttpGet]
	public string Index()
	{
		return "hello world";
	}
}