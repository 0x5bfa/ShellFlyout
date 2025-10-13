// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;
using ShellFlyout.Wasdk;
using System;

namespace U5BFA.ShellFlyout
{
	public partial class App : Application
	{
		public ShellFlyout? _trayIconFlyout;
		public TrayIconMenuFlyout? _trayIconMenuFlyout;

		private Window? _window;
		private SystemTrayIcon? _systemTrayIcon;
		private ContentBackdropManager? _backdropManager;

		public App()
		{
			InitializeComponent();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			_window = new MainWindow();
			_window.AppWindow.Hide();
			_window.DispatcherQueue.EnsureSystemDispatcherQueue();

			_systemTrayIcon = new SystemTrayIcon()
			{
				IconPath = "Assets\\TrayIcon.Dark.ico",
				Tooltip = "Shell flyout sample app (WASDK)",
				Id = new Guid("28DE460A-8BD6-4539-A406-5F685584FD4D")
			};

			_trayIconFlyout = new MainShellFlyout();
			_trayIconMenuFlyout = new(new MainTrayIconMeunFlyout());

			_systemTrayIcon.Show();
			_systemTrayIcon.LeftClicked += SystemTrayIcon_LeftClicked;
			_systemTrayIcon.RightClicked += SystemTrayIcon_RightClicked;
		}

		private async void SystemTrayIcon_LeftClicked(object? sender, EventArgs e)
		{
			if (_trayIconFlyout is null)
				return;

			if (_trayIconFlyout.IsOpen)
				await _trayIconFlyout.CloseFlyoutAsync();
			else
				await _trayIconFlyout.OpenFlyoutAsync();
		}

		private void SystemTrayIcon_RightClicked(object? sender, EventArgs e)
		{
			if (_trayIconMenuFlyout is null)
				return;

			_trayIconMenuFlyout.Show();
		}

		~App()
		{
			_systemTrayIcon?.LeftClicked -= SystemTrayIcon_LeftClicked;
			_systemTrayIcon?.Destroy();
			_trayIconFlyout?.Dispose();
			_backdropManager?.Dispose();
		}
	}
}
