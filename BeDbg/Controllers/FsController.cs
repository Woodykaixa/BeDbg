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

		var files = directory.GetFileSystemInfos().Select(file =>
			new FileModel(file.Name, Directory.Exists(file.FullName) ? FileType.Folder : FileType.File, file.FullName));
		return new DirectoryModel(directory.FullName, files);
	}
}