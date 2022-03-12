using BeDbg.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeDbg.Controllers;

[Route("[controller]")]
[ApiController]
public class FsController : ControllerBase
{
	[HttpGet("ls")]
	public DirectoryModel GetFilesByDir([FromQuery] string? dir)
	{
		var directory = new DirectoryInfo(dir ?? "C:/Users/" + Environment.UserName);
		if (directory == null)
		{
			throw new InvalidOperationException($"Cannot create directory info from [{dir}]");
		}

		var files = directory.GetFiles("*.exe").Select(file =>
			new FileModel(file.Name, FileType.File, file.FullName)).ToList();
		files.AddRange(directory.GetDirectories().Select(directoryInfo =>
			new FileModel(directoryInfo.Name, FileType.Folder, directoryInfo.FullName)));

		return new DirectoryModel(directory.FullName, files);
	}
}