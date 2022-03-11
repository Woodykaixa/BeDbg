namespace BeDbg.Models;

public record DirectoryModel(string Path, IEnumerable<FileModel> Files);