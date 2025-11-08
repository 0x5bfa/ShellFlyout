// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System;

namespace U5BFA.TrayIconFlyout
{
	internal class Program
	{
		[STAThread]
		static unsafe void Main()
		{
			var icon = new SystemTrayIcon()
			{
				IconPath = "Assets\\TrayIcon.Dark.ico",
				Tooltip = "TrayIconFlyout",
				Id = new Guid("28DE460A-8BD6-4539-A406-5F685584FD4D")
			};

			icon.Show();

			icon.LeftClicked += Icon_LeftClicked;

			var host = new XamlIslandWindow()
			{
				Height = 736,
				Width = 400
			};

			host.InitializeHost();
		}

		private static void Icon_LeftClicked(object? sender, MouseEventReceivedEventArgs e)
		{
			var host = new XamlIslandWindow()
			{
				Height = 736,
				Width = 400
			};

			host.InitializeHost();
		}
	}
}
