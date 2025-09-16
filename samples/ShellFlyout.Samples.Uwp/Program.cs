// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using Windows.System;
using Windows.UI.Xaml;

namespace Terat
{
	internal class Program
	{
		[STAThread]
		static unsafe void Main()
		{
			var icon = new SystemTrayIcon()
			{
				IconPath = "Assets\\TrayIcon.Dark.ico",
				Tooltip = "ShellFlyout"
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

		private static void Icon_LeftClicked(object? sender, EventArgs e)
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
