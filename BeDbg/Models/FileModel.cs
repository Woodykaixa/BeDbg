using System.ComponentModel;

namespace BeDbg.Models;

public class FileType
{
	public static readonly string File = "file";
	public static readonly string Folder = "folder";
}

public record FileModel(string Name, string Type, string Path);