// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;

namespace U5BFA.ShellFlyout
{
	public sealed partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			AppWindow.Resize(new(800, 600));
		}
	}
}
