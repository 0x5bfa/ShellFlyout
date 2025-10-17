// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;

namespace U5BFA.TrayIconFlyout
{
	public partial class App : Application
	{
		private Window? _window;

		public App()
		{
			InitializeComponent();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			_window = new MainWindow();
			_window.Activate();
			_window.DispatcherQueue.EnsureSystemDispatcherQueue();

			TrayIconManager.Default.Initialize();
		}

		~App()
		{
			TrayIconManager.Default.Dispose();
		}
	}
}
