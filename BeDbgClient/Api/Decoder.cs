using System;
using System.Runtime.InteropServices;

namespace BeDbgClient.Api;

public class Decoder : IDisposable
{
	[DllImport(Config.LibName, EntryPoint = "CreateDecoder")]
	private static extern UIntPtr CreateDecoder(Mode mode, AddressWidth addressWidth);

	[DllImport(Config.LibName, EntryPoint = "DestroyDecoder")]
	private static extern void DestroyDecoder(UIntPtr decoder);

	private readonly UIntPtr _decoderHandle;

	public Decoder(Mode mode, AddressWidth addressWidth)
	{
		_decoderHandle = CreateDecoder(mode, addressWidth);
	}

	/// <summary>
	/// Zydis decoder mode
	/// </summary>
	public enum Mode
	{
		Long64,
		LongCompat32,
		LongCompat16,
		Legacy32,
		Legacy16,
		Real16,
	}

	/// <summary>
	/// Zydis address width
	/// </summary>
	public enum AddressWidth
	{
		Width16,
		Width32,
		Width64,
	}

	private void ReleaseDecoder()
	{
		DestroyDecoder(_decoderHandle);
	}

	public void Dispose()
	{
		ReleaseDecoder();
	}

	~Decoder()
	{
		ReleaseDecoder();
	}
}