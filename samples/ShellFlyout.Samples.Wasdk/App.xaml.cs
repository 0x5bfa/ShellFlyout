// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System;

namespace U5BFA.ShellFlyout
{
	public partial class App : Application
	{
		public static ShellFlyout? MainShellFlyout { get; set; }

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
			_window.Activate();
			_window.DispatcherQueue.EnsureSystemDispatcherQueue();

			_systemTrayIcon = new SystemTrayIcon()
			{
				IconPath = "Assets\\TrayIcon.Dark.ico",
				Tooltip = "Shell flyout sample app (WASDK)",
				Id = new Guid("28DE460A-8BD6-4539-A406-5F685584FD4D")
			};

			MainShellFlyout = new MainShellFlyout();

			_systemTrayIcon.Show();
			_systemTrayIcon.LeftClicked += SystemTrayIcon_LeftClicked;
			//_systemTrayIcon.RightClicked += SystemTrayIcon_RightClicked;
		}

		private async void SystemTrayIcon_LeftClicked(object? sender, EventArgs e)
		{
			if (MainShellFlyout is null)
				return;

			if (MainShellFlyout.IsOpen)
				await MainShellFlyout.CloseFlyoutAsync();
			else
				await MainShellFlyout.OpenFlyoutAsync();
		}

		~App()
		{
			_systemTrayIcon?.LeftClicked -= SystemTrayIcon_LeftClicked;
			_systemTrayIcon?.Destroy();
			MainShellFlyout?.Dispose();
			_backdropManager?.Dispose();
		}
	}
}
