// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;
using System;

namespace Terat.Samples.Wasdk
{
	public partial class App : Application
	{
		private Window? _window;
		private ShellFlyout? _shellFlyout;

		public App()
		{
			InitializeComponent();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			_window = new MainWindow();
			_window.Activate();

			_shellFlyout = new() { Height = 736, Width = 384 };

			var icon = new SystemTrayIcon()
			{
				IconPath = "Assets\\TrayIcon.Dark.ico",
				Tooltip = "ShellFlyout",
				Id = new Guid("28DE460A-8BD6-4539-A406-5F685584FD4D")
			};

			icon.Show();
			icon.LeftClicked += SystemTrayIcon_LeftClicked;
		}

		private async void SystemTrayIcon_LeftClicked(object? sender, EventArgs e)
		{
			if (_shellFlyout is null)
				return;

			if (_shellFlyout.IsOpen)
				await _shellFlyout.CloseFlyoutAsync();
			else
				_shellFlyout.OpenFlyout();
		}
	}
}
