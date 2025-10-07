using Microsoft.UI.Xaml.Controls;

namespace U5BFA.ShellFlyout
{
	internal sealed partial class ShellFlyoutView : UserControl
	{
		public ShellFlyoutView()
		{
			InitializeComponent();
		}

		private void IncreaseSizeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			Height += 20;
			//Width += 20;
		}

		private void DecreaseSizeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			Height -= 20;
			//Width -= 20;
		}

		private void OrientationToggleSwitch_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			if (App.LeftClickShellFlyout is null)
				return;

			App.LeftClickShellFlyout.PopupDirection = OrientationToggleSwitch.IsOn ? Orientation.Horizontal : Orientation.Vertical;
		}
	}
}
