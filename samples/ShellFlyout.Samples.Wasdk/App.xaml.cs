// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace U5BFA.ShellFlyout
{
	public partial class App : Application
	{
		private Window? _window;
		private SystemTrayIcon? _systemTrayIcon;
		public static ShellFlyout? LeftClickShellFlyout { get; set; }
		//private ShellFlyout? _rightClickShellFlyout;
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
				?? throw new ArgumentNullException($"Failed to get a valid instance from {ContentBackdropManager.Create}.");

			_systemTrayIcon = new SystemTrayIcon()
			{
				IconPath = "Assets\\TrayIcon.Dark.ico",
				Tooltip = "ShellFlyout",
				Id = new Guid("28DE460A-8BD6-4539-A406-5F685584FD4D")
			};

			_systemTrayIcon.Show();
			_systemTrayIcon.LeftClicked += SystemTrayIcon_LeftClicked;
			_systemTrayIcon.RightClicked += SystemTrayIcon_RightClicked;

			LeftClickShellFlyout = new()
			{
				BackdropManager = _backdropManager,
				IsBackdropEnabled = true,
				Content = new ShellFlyoutView(),
				PopupDirection = Orientation.Vertical,
			};

			//_rightClickShellFlyout = new()
			//{
			//	IsBackdropEnabled = false,
			//	IsTransitionAnimationEnabled = false,
			//	Content = new ShellFlyoutView(),
			//	MenuFlyout = new()
			//	{
			//		Items =
			//		{
			//			new MenuFlyoutItem() { Text = "Item 1" },
			//			new MenuFlyoutItem() { Text = "Item 2" },
			//			new MenuFlyoutItem() { Text = "Item 3" },
			//		}
			//	},
			//};
		}

		private async void SystemTrayIcon_LeftClicked(object? sender, EventArgs e)
		{
			if (LeftClickShellFlyout is null)
				return;

			if (LeftClickShellFlyout.IsOpen)
				await LeftClickShellFlyout.CloseFlyoutAsync();
			else
				await LeftClickShellFlyout.OpenFlyoutAsync();
		}

		private async void SystemTrayIcon_RightClicked(object? sender, EventArgs e)
		{
			//if (_rightClickShellFlyout is null)
			//	return;

			//if (_rightClickShellFlyout.IsOpen)
			//	await _rightClickShellFlyout.CloseFlyoutAsync();
			//else
			//	await _rightClickShellFlyout.OpenFlyoutAsync();
		}

		~App()
		{
			_systemTrayIcon?.LeftClicked -= SystemTrayIcon_LeftClicked;
			_systemTrayIcon?.Destroy();
			LeftClickShellFlyout?.Dispose();
			_backdropManager?.Dispose();
		}
	}
}
