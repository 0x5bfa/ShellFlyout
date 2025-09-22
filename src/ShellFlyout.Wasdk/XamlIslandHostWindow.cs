// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Terat
{
	public partial class XamlIslandHostWindow : IDisposable
	{
		private readonly WNDPROC _wndProc;

		public HWND HWnd { get; private set; }
		public DesktopWindowXamlSource? DesktopWindowXamlSource { get; private set; }
		public required Rect Position { get; set; }

		public event EventHandler? WindowInactivated;

		public XamlIslandHostWindow()
		{
			_wndProc = new(WndProc);
		}

		public unsafe DesktopWindowXamlSource? Initialize()
		{
			fixed (char* pwszClassName = "ShellFlyoutHostClass", pwszWindowName = "ShellFlyoutHostWindow")
			{
				WNDCLASSW wndClass = default;
				wndClass.lpfnWndProc = (delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)Marshal.GetFunctionPointerForDelegate(_wndProc);
				wndClass.hInstance = PInvoke.GetModuleHandle(null);
				wndClass.lpszClassName = pwszClassName;
				PInvoke.RegisterClass(&wndClass);

				HWnd = PInvoke.CreateWindowEx(
					WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP | WINDOW_EX_STYLE.WS_EX_TOOLWINDOW, pwszClassName, pwszWindowName,
					WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_VISIBLE | WINDOW_STYLE.WS_SYSMENU,
					(int)Position.X, (int)Position.Y, (int)Position.Width, (int)Position.Height,
					HWND.Null, HMENU.Null, wndClass.hInstance, null);
			}

			DesktopWindowXamlSource = new();
			DesktopWindowXamlSource.Initialize(Win32Interop.GetWindowIdFromWindow(HWnd));

			return DesktopWindowXamlSource;
		}

		private LRESULT WndProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
		{
			switch (uMsg)
			{
				case PInvoke.WM_ACTIVATE:
					{
						if (LOWORD((nint)(nuint)wParam) == PInvoke.WA_INACTIVE)
							WindowInactivated?.Invoke(this, EventArgs.Empty);
					}
					break;
				default:
					{
						return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
					}
			}

			return (LRESULT)0;
		}

		public void Dispose()
		{
			PInvoke.DestroyWindow(HWnd);
			DesktopWindowXamlSource?.Dispose();
			DesktopWindowXamlSource = null;
		}
	}
}
