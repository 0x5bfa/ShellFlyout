// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace Terat.Samples.Wasdk
{
	public partial class App : Application
	{
		private Window? _window;
		private SystemTrayIcon? _systemTrayIcon;
		private ShellFlyout? _shellFlyout;
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
			_backdropManager = ContentBackdropManager.Create(new DesktopAcrylicController(), _window.Compositor, ((FrameworkElement)_window.Content).ActualTheme)
				?? throw new ArgumentNullException();

			_systemTrayIcon = new SystemTrayIcon()
			{
				IconPath = "Assets\\TrayIcon.Dark.ico",
				Tooltip = "ShellFlyout",
				Id = new Guid("28DE460A-8BD6-4539-A406-5F685584FD4D")
			};

			_systemTrayIcon.Show();
			_systemTrayIcon.LeftClicked += SystemTrayIcon_LeftClicked;

			_shellFlyout = new() { Height = 736, Width = 384, BackdropManager = _backdropManager };
		}

		private async void SystemTrayIcon_LeftClicked(object? sender, EventArgs e)
		{
			if (_shellFlyout is null)
				return;

			if (_shellFlyout.IsOpen)
				await _shellFlyout.CloseFlyoutAsync();
			else
				await _shellFlyout.OpenFlyoutAsync();
		}

		~App()
		{
			_systemTrayIcon?.LeftClicked -= SystemTrayIcon_LeftClicked;
			_systemTrayIcon?.Destroy();
			_shellFlyout?.Dispose();
			_backdropManager?.Dispose();
		}
	}
}
