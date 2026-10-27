using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KidFansChannel.GalCore
{
	internal class NativeMethods
	{
		internal const int PATH_MAX = 260;

		internal const int CSIDL_PROFILE = 40;

		[DllImport("shfolder.dll", CharSet = CharSet.Auto)]
		internal static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

		/// <summary>投递窗口消息，用来把「停止视频」这类请求交给消息线程处理。</summary>
		[DllImport("user32.dll")]
		internal static extern IntPtr PostMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
	}
}
