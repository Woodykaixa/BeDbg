using System.Runtime.InteropServices;
using BeDbg.Api;
using BeDbg.Contexts;
using BeDbg.Models;

namespace BeDbg.Services;

public class DebugService
{
	private readonly DebuggingProcessContext _ctx;

	public DebugService(DebuggingProcessContext ctx)
	{
		_ctx = ctx;
	}

	public ValueTask<DebuggingProcess?> FindOneByPid(int pid)
	{
		return _ctx.DebuggingProcesses.FindAsync(pid);
	}

	public async Task<DebuggingProcess> AttachProcess(int pid)
	{
		BeDbg64.ClearError();
		var handle = BeDbg64.AttachProcess(pid);
		var process = new DebuggingProcess
		{
			Id = pid,
			AttachTime = DateTime.Now,
			Handle = handle.ToInt64()
		};
		_ctx.DebuggingProcesses.Add(process);
		await _ctx.SaveChangesAsync();
		return process;
	}

	public IEnumerable<DebuggingProcess> ListDebuggingProcesses() => _ctx.DebuggingProcesses.ToList();

	public async Task<bool> DetachProcess(int pid)
	{
		BeDbg64.ClearError();
		var process = await _ctx.DebuggingProcesses.FindAsync(pid);
		if (process == null)
		{
			return true;
		}

		if (!BeDbg64.DetachProcess(new IntPtr(process.Handle)))
		{
			return false;
		}

		_ctx.DebuggingProcesses.Remove(process);
		await _ctx.SaveChangesAsync();

		return true;
	}
}