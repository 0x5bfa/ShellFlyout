// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System;

namespace U5BFA.TrayIconFlyout
{
	internal partial class TrayIconManager : IDisposable
	{
		private static readonly Lazy<TrayIconManager> _default = new(() => new TrayIconManager());
		internal static TrayIconManager Default => _default.Value;

		private SystemTrayIcon? _systemTrayIcon;
		private TrayIconMenuFlyout? _trayIconMenuFlyout;

		internal TrayIconFlyout? TrayIconFlyout { get; set; }

		private TrayIconManager() { }

		internal void Initialize()
		{
			_systemTrayIcon = new SystemTrayIcon()
			{
				IconPath = "Assets\\Tray.ico",
				Tooltip = "Shell flyout sample app (WASDK)",
				Id = new Guid("28DE460A-8BD6-4539-A406-5F685584FD4D")
			};

			TrayIconFlyout = new MainTrayIconFlyout();
			_trayIconMenuFlyout = new MainTrayIconMeunFlyout();

			_systemTrayIcon.Show();
			_systemTrayIcon.LeftClicked += SystemTrayIcon_LeftClicked;
			_systemTrayIcon.RightClicked += SystemTrayIcon_RightClicked;
		}

		private void SystemTrayIcon_LeftClicked(object? sender, MouseEventReceivedEventArgs e)
		{
			if (TrayIconFlyout is null)
				return;

			if (TrayIconFlyout.IsOpen)
				TrayIconFlyout.Hide();
			else
				TrayIconFlyout.Show();
		}

		private void SystemTrayIcon_RightClicked(object? sender, MouseEventReceivedEventArgs e)
		{
			if (_trayIconMenuFlyout is null)
				return;

			if (_trayIconMenuFlyout.IsOpen)
				_trayIconMenuFlyout.Hide();

			_trayIconMenuFlyout.Show(e.Point);
		}

		public void Dispose()
		{
			_systemTrayIcon?.LeftClicked -= SystemTrayIcon_LeftClicked;
			_systemTrayIcon?.RightClicked -= SystemTrayIcon_RightClicked;
			_systemTrayIcon?.Destroy();
			TrayIconFlyout?.Dispose();
		}
	}
}
