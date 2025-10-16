// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System;

namespace U5BFA.ShellFlyout
{
	internal partial class TrayIconManager : IDisposable
	{
		private static readonly Lazy<TrayIconManager> _default = new(() => new TrayIconManager());
		public static TrayIconManager Default => _default.Value;

		private static SystemTrayIcon? _systemTrayIcon;
		private static ContentBackdropManager? _backdropManager;
		private static ShellFlyout? _trayIconFlyout;
		private static TrayIconMenuFlyout? _trayIconMenuFlyout;

		private TrayIconManager() { }

		internal void Initialize()
		{
			_systemTrayIcon = new SystemTrayIcon()
			{
				IconPath = "Assets\\TrayIcon.Dark.ico",
				Tooltip = "Shell flyout sample app (WASDK)",
				Id = new Guid("28DE460A-8BD6-4539-A406-5F685584FD4D")
			};

			_trayIconFlyout = new MainTrayIconFlyout();
			_trayIconMenuFlyout = new MainTrayIconMeunFlyout();

			_systemTrayIcon.Show();
			_systemTrayIcon.LeftClicked += SystemTrayIcon_LeftClicked;
			_systemTrayIcon.RightClicked += SystemTrayIcon_RightClicked;
		}

		private void SystemTrayIcon_LeftClicked(object? sender, EventArgs e)
		{
			if (_trayIconFlyout is null)
				return;

			if (_trayIconFlyout.IsOpen)
				_trayIconFlyout.Hide();
			else
				_trayIconFlyout.Show();
		}

		private void SystemTrayIcon_RightClicked(object? sender, EventArgs e)
		{
			if (_trayIconMenuFlyout is null)
				return;

			if (_trayIconMenuFlyout.IsOpen)
				_trayIconMenuFlyout.Hide();

			_trayIconMenuFlyout.Show();
		}

		public void Dispose()
		{
			_systemTrayIcon?.LeftClicked -= SystemTrayIcon_LeftClicked;
			_systemTrayIcon?.RightClicked -= SystemTrayIcon_RightClicked;
			_systemTrayIcon?.Destroy();
			_trayIconFlyout?.Dispose();
			_backdropManager?.Dispose();
		}
	}
}
