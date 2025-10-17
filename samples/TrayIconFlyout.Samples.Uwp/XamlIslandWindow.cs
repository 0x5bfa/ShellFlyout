// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.UI.Core;
using Windows.UI.Xaml.Hosting;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.WinRT;
using Windows.Win32.System.WinRT.Xaml;
using Windows.Win32.UI.WindowsAndMessaging;
using WinRT;
using static Windows.Win32.ManualDefinitions;

namespace U5BFA.TrayIconFlyout
{
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate LRESULT WNDPROC([In] HWND hWnd, [In] uint uMsg, [In] WPARAM wParam, [In] LPARAM lParam);

	public unsafe class XamlIslandWindow
	{
		private App _xamlApp = null;

		private WNDPROC _wndProc = default;

		private HWND _hwnd = default;
		private HWND _xamlHwnd = default;
		private HWND _coreHwnd = default;

		private bool _xamlInitialized = false;

		private DesktopWindowXamlSource _desktopWindowXamlSource = null;
		private WindowsXamlManager _xamlManager = null;
		private CoreWindow _coreWindow = null;

		private ComPtr<IDesktopWindowXamlSourceNative2> _pDesktopWindowXamlSourceNative2 = default;

		public required int Width { get; set; }

		public required int Height { get; set; }

		public XamlIslandWindow() { }

		public void InitializeHost()
		{
			fixed (char* pwszClassName = "TrayIconFlyoutHostClass", pwszWindowName = "TrayIconFlyoutHostWindow")
			{
				_wndProc = new(WndProc);

				WNDCLASSW wndClass = default;
				wndClass.lpfnWndProc = (delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)Marshal.GetFunctionPointerForDelegate(_wndProc);
				wndClass.hInstance = PInvoke.GetModuleHandle(null);
				wndClass.lpszClassName = pwszClassName;
				PInvoke.RegisterClass(&wndClass);

				PInvoke.CreateWindowEx(
					WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP | WINDOW_EX_STYLE.WS_EX_TOOLWINDOW, pwszClassName, pwszWindowName,
					WINDOW_STYLE.WS_OVERLAPPEDWINDOW | WINDOW_STYLE.WS_VISIBLE,
					PInvoke.CW_USEDEFAULT, PInvoke.CW_USEDEFAULT, Width, Height,
					HWND.Null, HMENU.Null, wndClass.hInstance, null);
			}

			MSG msg;
			while (PInvoke.GetMessage(&msg, HWND.Null, 0, 0))
			{
				bool xamlSourceProcessedMessage = _xamlApp is not null && PreTranslateMessage(&msg);
				if (!xamlSourceProcessedMessage)
				{
					PInvoke.TranslateMessage(&msg);
					PInvoke.DispatchMessage(&msg);
				}
			}
		}

		public DesktopWindowXamlSource InitializeXamlIsland()
		{
			// Is this needed anymore? maybe for older builds?
			fixed (char* pwszTwinApiAppCoreDll = "twinapi.appcore.dll", pwszThreadPoolWinRTDll = "threadpoolwinrt.dll")
			{
				PInvoke.LoadLibrary(pwszTwinApiAppCoreDll);
				PInvoke.LoadLibrary(pwszThreadPoolWinRTDll);
			}

			_xamlManager = WindowsXamlManager.InitializeForCurrentThread();
			_desktopWindowXamlSource = new();

			Guid IID_IDesktopWindowXamlSourceNative2 = IDesktopWindowXamlSourceNative2.IID_Guid;
			Guid IID_ICoreWindowInterop = ICoreWindowInterop.IID_Guid;

			((IUnknown*)((IWinRTObject)_desktopWindowXamlSource).NativeObject.ThisPtr)->QueryInterface(&IID_IDesktopWindowXamlSourceNative2, (void**)_pDesktopWindowXamlSourceNative2.GetAddressOf());

			// For extra safety
			GC.KeepAlive(_desktopWindowXamlSource);

			_pDesktopWindowXamlSourceNative2.Get()->AttachToWindow(_hwnd);
			_pDesktopWindowXamlSourceNative2.Get()->get_WindowHandle((HWND*)Unsafe.AsPointer(ref _xamlHwnd));

			RECT wRect;
			PInvoke.GetClientRect(_hwnd, &wRect);
			PInvoke.SetWindowPos(_xamlHwnd, HWND.Null, 0, 0, wRect.right - wRect.left, wRect.bottom - wRect.top, SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);

			_coreWindow = CoreWindow.GetForCurrentThread();

			using ComPtr<ICoreWindowInterop> pCoreWindowInterop = default;
			((IUnknown*)((IWinRTObject)_coreWindow).NativeObject.ThisPtr)->QueryInterface(&IID_ICoreWindowInterop, (void**)pCoreWindowInterop.GetAddressOf());
			pCoreWindowInterop.Get()->get_WindowHandle((HWND*)Unsafe.AsPointer(ref _coreHwnd));

			_xamlInitialized = true;

			return _desktopWindowXamlSource;
		}

		private bool PreTranslateMessage(MSG* msg)
		{
			BOOL result = false;

			if (_xamlInitialized)
				_pDesktopWindowXamlSourceNative2.Get()->PreTranslateMessage(msg, &result);

			return result;
		}

		private LRESULT WndProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
		{
			switch (uMsg)
			{
				case PInvoke.WM_CREATE:
					{
						WINDOW_STYLE lStyle = (WINDOW_STYLE)PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
						lStyle &= ~(WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_MINIMIZEBOX | WINDOW_STYLE.WS_MAXIMIZEBOX | WINDOW_STYLE.WS_SYSMENU);
						PInvoke.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)lStyle);

						PInvoke.RoInitialize(RO_INIT_TYPE.RO_INIT_SINGLETHREADED);

						_hwnd = hWnd;
						_xamlApp = new(this);
					}
					break;
				case PInvoke.WM_SIZE:
					{
						var x = LOWORD(lParam);
						var y = HIWORD(lParam);

						if (_xamlHwnd != default)
							PInvoke.SetWindowPos(_xamlHwnd, HWND.Null, 0, 0, x, y, SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);

						if (_coreHwnd != default)
							PInvoke.SendMessage(_coreHwnd, PInvoke.WM_SIZE, (WPARAM)(nuint)x, y);
					}
					break;
				case PInvoke.WM_SETTINGCHANGE:
				case PInvoke.WM_THEMECHANGED:
					{
						// Process CoreWindow message
						if (_coreHwnd != default)
							PInvoke.SendMessage(_coreHwnd, uMsg, wParam, lParam);
					}
					break;
				case PInvoke.WM_SETFOCUS:
					{
						if (_xamlHwnd != default)
							PInvoke.SetFocus(_xamlHwnd);
					}
					break;
				case PInvoke.WM_ACTIVATE:
					{
						if (LOWORD((nint)(nuint)wParam) == PInvoke.WA_INACTIVE)
						{
							Debug.WriteLine("Inactive window.");
						}
						else
						{
						}
					}
					break;
				case PInvoke.WM_DESTROY:
					{
						_xamlApp = null;
						PInvoke.PostQuitMessage(0);
					}
					break;
				default:
					{
						return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
					}
			}

			return (LRESULT)0;
		}
	}
}
