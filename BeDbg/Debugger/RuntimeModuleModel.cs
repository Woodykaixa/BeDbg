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
	}

	public void LoadModuleName(ProcessModel process)
	{
		if (ImageNamePtr == IntPtr.Zero)
		{
			return;
		}

		//
		var buffer = process.ReadMemory(ImageNamePtr, 260);
		var sb = new StringBuilder(260);
		var i = 0;
		while (i < buffer.Length)
		{
			var ch = (char) buffer[i];
			if (ch == '\0')
			{
				break;
			}

			sb.Append(ch);
			i++;
		}


		ImageName = sb.ToString();
		Console.WriteLine($"Format name of {ImageBase.ToInt64()}: {ImageName}");
	}

	~RuntimeModuleModel()
	{
		Kernel.CloseHandle(ImageFile);
	}
}