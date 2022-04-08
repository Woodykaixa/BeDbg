using BeDbg.Api;

namespace BeDbg.Debugger;

/// <summary>
/// <see cref="BaseDebugger"/> uses this class to interact with Windows PE Module
/// </summary>
public class RuntimeModuleModel
{
	public IntPtr ImageFile { get; }
	public IntPtr ImageName { get; }

	public IntPtr ImageBase { get; set; }
	public uint DebugInfoOffset { get; set; }
	public uint DebugInfoSize { get; set; }

	private bool ImageNameEncodingUnicode { get; }


	public RuntimeModuleModel(IntPtr imageFile, IntPtr imageName, ushort unicodeFlag)
	{
		ImageFile = imageFile;
		ImageName = imageName;
		ImageNameEncodingUnicode = unicodeFlag != 0;
	}

	~RuntimeModuleModel()
	{
		Kernel.CloseHandle(ImageFile);
	}
}