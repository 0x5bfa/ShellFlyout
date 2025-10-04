// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace U5BFA
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await Task.Delay(1000);
			VisualStateManager.GoToState(this, "Visible", true);
		}
	}
}
