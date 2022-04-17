using System.Text;
using BeDbg.Api;
using BeDbg.Models;

namespace BeDbg.Debugger;

/// <summary>
/// <see cref="BaseDebugger"/> uses this class to interact with Windows PE Module
/// </summary>
public class RuntimeModuleModel
{
	public IntPtr ImageFile { get; }
	public IntPtr ImageNamePtr { get; }
	public string ImageName { get; set; } = string.Empty;
	public IntPtr ImageBase { get; set; }
	public uint DebugInfoOffset { get; set; }
	public uint DebugInfoSize { get; set; }

	private bool ImageNameEncodingUnicode { get; }


	public RuntimeModuleModel(IntPtr imageFile, IntPtr imageNamePtr, ushort unicodeFlag)
	{
		ImageFile = imageFile;
		ImageNamePtr = imageNamePtr;
		ImageNameEncodingUnicode = unicodeFlag != 0;

		var sb = new StringBuilder(256);
		var result = Kernel.GetFinalPathNameByHandle(ImageFile, sb, 256, 0x0);
		if (result)
		{
			ImageName = sb.ToString().Trim('\n', '\t', ' ');
		}
	}
	
	~RuntimeModuleModel()
	{
		Kernel.CloseHandle(ImageFile);
	}
}