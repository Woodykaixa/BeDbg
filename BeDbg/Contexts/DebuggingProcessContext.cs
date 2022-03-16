using BeDbg.Models;
using Microsoft.EntityFrameworkCore;

namespace BeDbg.Contexts;

public class DebuggingProcessContext : DbContext
{
	public DebuggingProcessContext(DbContextOptions<DebuggingProcessContext> options) : base(options)
	{
	}

	public DbSet<DebuggingProcess> DebuggingProcesses { get; set; } = null!;
}