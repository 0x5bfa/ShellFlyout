// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;

namespace Terat.Samples.Wasdk
{
	public sealed partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			AcrylicBackdropController.Initialize(this);
		}
	}
}
