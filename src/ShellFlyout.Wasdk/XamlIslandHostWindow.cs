// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace U5BFA.ShellFlyout
{
	internal unsafe partial class XamlIslandHostWindow : IDisposable
	{
		private const string WindowClassName = "ShellFlyoutHostClass";
		private const string WindowName = "ShellFlyoutHostWindow";

		private readonly WNDPROC _wndProc;

		internal HWND HWnd
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		internal DesktopWindowXamlSource? DesktopWindowXamlSource
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		internal Rect WindowSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				RECT rect;
				PInvoke.GetWindowRect(HWnd, &rect);
				return new(rect.X, rect.Y, rect.Width, rect.Height);
			}
		}

		internal event EventHandler? WindowInactivated;

		internal XamlIslandHostWindow()
		{
			_wndProc = new(WndProc);
		}

		internal void Initialize(UIElement content)
		{
			WNDCLASSW wndClass = default;
			wndClass.lpfnWndProc = (delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)Marshal.GetFunctionPointerForDelegate(_wndProc);
			wndClass.hInstance = PInvoke.GetModuleHandle(null);
			wndClass.lpszClassName = (PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowClassName.GetPinnableReference()));
			PInvoke.RegisterClass(&wndClass);

			HWnd = PInvoke.CreateWindowEx(
				WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP | WINDOW_EX_STYLE.WS_EX_TOOLWINDOW | WINDOW_EX_STYLE.WS_EX_TOPMOST,
				(PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowClassName.GetPinnableReference())),
				(PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowName.GetPinnableReference())),
				WINDOW_STYLE.WS_POPUP, 0, 0, 0, 0, HWND.Null, HMENU.Null, wndClass.hInstance, null);

			PInvoke.SetLayeredWindowAttributes(HWnd, (COLORREF)0, 255, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);

			DesktopWindowXamlSource = new();
			DesktopWindowXamlSource.Initialize(Win32Interop.GetWindowIdFromWindow(HWnd));
			DesktopWindowXamlSource.Content = content;

			PInvoke.SetLayeredWindowAttributes((HWND)DesktopWindowXamlSource.SiteBridge.WindowId.Value, (COLORREF)0, 255, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_ALPHA);
		}

		internal void MoveAndResize(RectInt32 rect)
		{
			if (DesktopWindowXamlSource is null)
				return;

			PInvoke.SetWindowPos(HWnd, HWND.HWND_TOP, rect.X, rect.Y, rect.Width, rect.Height, 0U);
			PInvoke.SetWindowPos((HWND)DesktopWindowXamlSource.SiteBridge.WindowId.Value, HWND.HWND_TOP, 0, 0, rect.Width, rect.Height, 0U);
		}

		internal void Maximize()
		{
			if (DesktopWindowXamlSource is null)
				return;

			var bottomRightPoint = WindowHelpers.GetBottomRightCornerPoint();
			PInvoke.SetWindowPos(HWnd, HWND.HWND_TOP, 0, 0, bottomRightPoint.X, bottomRightPoint.Y, 0U);

			PInvoke.SetWindowPos((HWND)DesktopWindowXamlSource.SiteBridge.WindowId.Value, HWND.HWND_TOP, 0, 0, bottomRightPoint.X, bottomRightPoint.Y, 0U);
		}

		internal void SetHWndRectRegion(RectInt32 rect)
		{
			if (DesktopWindowXamlSource is null)
				return;

			HRGN region = PInvoke.CreateRectRgn(rect.X, rect.Y, rect.Width, rect.Height);
			PInvoke.SetWindowRgn(HWnd, region, false);
			PInvoke.SetWindowRgn((HWND)DesktopWindowXamlSource.SiteBridge.WindowId.Value, region, false);
		}

		internal void UpdateWindowVisibility(bool isVisible)
		{
			PInvoke.ShowWindow(HWnd, isVisible ? SHOW_WINDOW_CMD.SW_SHOW : SHOW_WINDOW_CMD.SW_HIDE);
			if (isVisible) DesktopWindowXamlSource?.SiteBridge.Show(); else DesktopWindowXamlSource?.SiteBridge.Hide();
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

		public unsafe void Dispose()
		{
			PInvoke.DestroyWindow(HWnd);
			PInvoke.UnregisterClass((PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowClassName.GetPinnableReference())), PInvoke.GetModuleHandle(null));
			DesktopWindowXamlSource?.Dispose();
			DesktopWindowXamlSource = null;
		}
	}
}
