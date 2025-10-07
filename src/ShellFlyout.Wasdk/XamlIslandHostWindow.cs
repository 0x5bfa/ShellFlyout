// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace U5BFA.ShellFlyout
{
	public unsafe partial class XamlIslandHostWindow : IDisposable
	{
		private const string WindowClassName = "ShellFlyoutHostClass";
		private const string WindowName = "ShellFlyoutHostWindow";

		private readonly WNDPROC _wndProc;

		public HWND HWnd { get; private set; }
		public DesktopWindowXamlSource? DesktopWindowXamlSource { get; private set; }

		public event EventHandler? WindowInactivated;

		public XamlIslandHostWindow()
		{
			_wndProc = new(WndProc);
		}

		public void Initialize(UIElement content)
		{
			WNDCLASSW wndClass = default;
			wndClass.lpfnWndProc = (delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)Marshal.GetFunctionPointerForDelegate(_wndProc);
			wndClass.hInstance = PInvoke.GetModuleHandle(null);
			wndClass.lpszClassName = (PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowClassName.GetPinnableReference()));
			PInvoke.RegisterClass(&wndClass);

			HWnd = PInvoke.CreateWindowEx(
				WINDOW_EX_STYLE.WS_EX_TOOLWINDOW | WINDOW_EX_STYLE.WS_EX_LAYERED,
				(PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowClassName.GetPinnableReference())),
				(PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowName.GetPinnableReference())),
				WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_VISIBLE | WINDOW_STYLE.WS_SYSMENU,
				0, 0, 0, 0, HWND.Null, HMENU.Null, wndClass.hInstance, null);

			PInvoke.SetLayeredWindowAttributes(HWnd, (COLORREF)0, 255, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);

			DesktopWindowXamlSource = new();
			DesktopWindowXamlSource.Initialize(Win32Interop.GetWindowIdFromWindow(HWnd));
			DesktopWindowXamlSource.Content = content;
		}

		public void Resize(Rect rect, bool coerceShow)
		{
			if (DesktopWindowXamlSource is null)
				return;

			DesktopWindowXamlSource.SiteBridge.Show();

			var wasVisible = PInvoke.IsWindowVisible(HWnd);
			PInvoke.SetWindowPos(
				HWnd, HWND.HWND_TOP, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height,
				(coerceShow || (!coerceShow && wasVisible) ? SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW : SET_WINDOW_POS_FLAGS.SWP_HIDEWINDOW) | SET_WINDOW_POS_FLAGS.SWP_NOREDRAW);
		}

		public void Minimize()
		{
			if (DesktopWindowXamlSource is null)
				return;

			DesktopWindowXamlSource.SiteBridge.Hide();

			PInvoke.SetWindowPos(HWnd, HWND.HWND_TOP, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_HIDEWINDOW);
		}

		public void UpdateWindowVisibility(bool isVisible)
		{
			PInvoke.ShowWindow(HWnd, isVisible ? SHOW_WINDOW_CMD.SW_SHOW : SHOW_WINDOW_CMD.SW_HIDE);
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
				case PInvoke.WM_SIZE:
					{
						//if (DesktopWindowXamlSource is null)
						//	break;

						//int x = LOWORD(lParam);
						//int y = HIWORD(lParam);
						//DesktopWindowXamlSource.SiteBridge.MoveAndResize(new(0, 300, x, y));

						//HRGN region = PInvoke.CreateRectRgn(0, 300, x, y);
						//PInvoke.SetWindowRgn(HWnd, region, false);
					}
					break;
				default:
					{
						return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
					}
			}

			return (LRESULT)0;
		}

		public unsafe void Dispose()
		{
			PInvoke.DestroyWindow(HWnd);
			PInvoke.UnregisterClass((PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowClassName.GetPinnableReference())), PInvoke.GetModuleHandle(null));
			DesktopWindowXamlSource?.Dispose();
			DesktopWindowXamlSource = null;
		}
	}
}
