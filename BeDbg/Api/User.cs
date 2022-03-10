using System.Runtime.InteropServices;

namespace BeDbg.Api;

public class User
{
	[DllImport(InteropConfig.User, EntryPoint = "GetWindowTextLengthW")]
	public static extern int GetWindowTextLength(IntPtr handle);

	[DllImport(InteropConfig.User, EntryPoint = "GetWindowTextW")]
	public static extern unsafe int GetWindowTextW(IntPtr handle, char* title, int maxCount);

	public delegate bool EnumWindowsProc(IntPtr handle, IntPtr param);

	[DllImport(InteropConfig.User, EntryPoint = "EnumWindows")]
	public static extern int EnumWindows(EnumWindowsProc proc, IntPtr param);

	[DllImport(InteropConfig.User, EntryPoint = "GetWindowThreadProcessId")]
	public static extern unsafe int GetWindowThreadProcessId(IntPtr handle, int* processId);

	[DllImport(InteropConfig.User, EntryPoint = "IsWindowVisible")]
	public static extern bool IsWindowVisible(IntPtr handle);

	[DllImport(InteropConfig.User, EntryPoint = "GetWindow")]
	public static extern IntPtr GetWindow(IntPtr handle, int command);

	public struct MainWindowFinder
	{
		private const int GW_OWNER = 4;
		private IntPtr _bestHandle;
		private int _processId;

		public static unsafe IntPtr FindMainWindow(int processId)
		{
			MainWindowFinder instance;

			instance._bestHandle = IntPtr.Zero;
			instance._processId = processId;

			EnumWindows((handle, extraParameter) =>
			{
				MainWindowFinder* instance = (MainWindowFinder*) extraParameter;

				int processId = 0; // Avoid uninitialized variable if the window got closed in the meantime
				GetWindowThreadProcessId(handle, &processId);

				if ((processId == instance->_processId) && IsMainWindow(handle))
				{
					instance->_bestHandle = handle;
					return false;
				}

				return true;
			}, (IntPtr) (&instance));

			return instance._bestHandle;
		}

		private static bool IsMainWindow(IntPtr handle)
		{
			return (GetWindow(handle, GW_OWNER) == IntPtr.Zero) &&
			       IsWindowVisible(handle) != false;
		}

		private static unsafe bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
		{
			MainWindowFinder* instance = (MainWindowFinder*) extraParameter;

			int processId = 0; // Avoid uninitialized variable if the window got closed in the meantime
			GetWindowThreadProcessId(handle, &processId);

			if ((processId == instance->_processId) && IsMainWindow(handle))
			{
				instance->_bestHandle = handle;
				return false;
			}

			return true;
		}
	}
}