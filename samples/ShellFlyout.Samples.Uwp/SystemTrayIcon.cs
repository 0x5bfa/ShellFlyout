// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Terat
{
	public unsafe class SystemTrayIcon
	{
		private const uint WM_UNIQUE_MESSAGE = 2048U;

		private readonly Guid _guid = new("E85EABCB-6C3B-4550-A89A-987A05A345AF");
		private readonly uint _taskbarRestartMessageId;
		private readonly WNDPROC _wndProc;
		private readonly HWND _hWnd = default;

		private bool _created;

		public required string IconPath { get; set; }
		public required string Tooltip { get; set; }

		public event EventHandler? LeftClicked;
		public event EventHandler? RightClicked;

		public SystemTrayIcon()
		{
			fixed (char* pwszTaskbarCreatedWMName = "TaskbarCreated")
				_taskbarRestartMessageId = PInvoke.RegisterWindowMessage(pwszTaskbarCreatedWMName);

			fixed (char* pwszClassName = $"SystemTrayIcon_{_guid:B}")
			{
				_wndProc = new(WndProc);

				WNDCLASSW wndClass = default;
				wndClass.style = WNDCLASS_STYLES.CS_DBLCLKS;
				wndClass.lpfnWndProc = (delegate* unmanaged[Stdcall]<HWND, uint, WPARAM, LPARAM, LRESULT>)Marshal.GetFunctionPointerForDelegate(_wndProc);
				wndClass.hInstance = PInvoke.GetModuleHandle(null);
				wndClass.lpszClassName = pwszClassName;
				PInvoke.RegisterClass(&wndClass);

				_hWnd = PInvoke.CreateWindowEx(
					WINDOW_EX_STYLE.WS_EX_LEFT, pwszClassName, null, WINDOW_STYLE.WS_OVERLAPPED,
					0, 0, 1, 1, HWND.Null, HMENU.Null, HINSTANCE.Null, null);
			}
		}

		public void Show()
		{
			HICON hIcon = default;
			fixed (char* pwszIconPath = IconPath)
				hIcon = (HICON)(void*)PInvoke.LoadImage(HINSTANCE.Null, pwszIconPath, GDI_IMAGE_TYPE.IMAGE_ICON, 0, 0, IMAGE_FLAGS.LR_LOADFROMFILE | IMAGE_FLAGS.LR_DEFAULTSIZE);

			NOTIFYICONDATAW data = default;
			data.cbSize = (uint)sizeof(NOTIFYICONDATAW);
			data.hWnd = _hWnd;
			data.uCallbackMessage = WM_UNIQUE_MESSAGE;
			data.hIcon = hIcon;
			data.guidItem = _guid;
			data.uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP | NOTIFY_ICON_DATA_FLAGS.NIF_GUID | NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP;
			data.szTip = Tooltip ?? string.Empty;

			if (_created)
			{
				PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, &data);
			}
			else
			{
				PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, &data);
				PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, &data);

				data.Anonymous.uVersion = 4u;

				// Set the icon handler version
				// NOTE: Do not omit this code. If you remove, the icon won't be shown.
				PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_SETVERSION, &data);

				_created = true;
			}
		}

		public void Destroy()
		{
			if (_created)
			{
				_created = false;

				NOTIFYICONDATAW data = default;
				data.cbSize = (uint)sizeof(NOTIFYICONDATAW);
				data.hWnd = _hWnd;
				data.guidItem = _guid;
				data.uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP | NOTIFY_ICON_DATA_FLAGS.NIF_GUID | NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP;

				// Delete the existing icon
				PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, &data);
			}
		}

		internal LRESULT WndProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
		{
			switch (uMsg)
			{
				case WM_UNIQUE_MESSAGE:
					{
						switch ((uint)(lParam.Value & 0xFFFF))
						{
							case PInvoke.WM_LBUTTONUP:
								{
									PInvoke.SetForegroundWindow(hWnd);
									LeftClicked?.Invoke(this, EventArgs.Empty);

									break;
								}
							case PInvoke.WM_RBUTTONUP:
								{
									RightClicked?.Invoke(this, EventArgs.Empty);

									break;
								}
						}

						break;
					}
				case PInvoke.WM_DESTROY:
					{
						Destroy();

						break;
					}
				default:
					{
						if (uMsg == _taskbarRestartMessageId)
						{
							Destroy();
							Show();
						}

						return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
					}
			}
			return default;
		}
	}
}
