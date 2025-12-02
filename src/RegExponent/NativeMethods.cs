namespace RegExponent;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

internal static class NativeMethods
{
	// https://stackoverflow.com/a/25139586/1882616
	private const int WM_SETICON = 0x0080;
	private const int ICON_SMALL = 0;
	private const int ICON_BIG = 1;

	private const int GWL_EXSTYLE = -20;
	private const int WS_EX_DLGMODALFRAME = 0x0001;
	private const int SWP_NOSIZE = 0x0001;
	private const int SWP_NOMOVE = 0x0002;
	private const int SWP_NOZORDER = 0x0004;
	private const int SWP_FRAMECHANGED = 0x0020;

	public static void RemoveIcon(Window window)
	{
		IntPtr hWnd = new WindowInteropHelper(window).Handle;

		// Change the extended window style to not show a window icon
		int extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
		SetWindowLong(hWnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

		// Update the window's non-client area to reflect the changes
		SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

		SendMessage(hWnd, WM_SETICON, ICON_SMALL, IntPtr.Zero);
		SendMessage(hWnd, WM_SETICON, ICON_BIG, IntPtr.Zero);
	}

	[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	[DllImport("User32.dll")]
	private static extern int GetWindowLong(IntPtr hwnd, int index);

	[DllImport("User32.dll")]
	private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

	[DllImport("User32.dll")]
	private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);
}
