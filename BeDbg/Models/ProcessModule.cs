namespace BeDbg.Models;

public record ProcessModule(string Name, ulong Entry, uint Size, ulong Base);