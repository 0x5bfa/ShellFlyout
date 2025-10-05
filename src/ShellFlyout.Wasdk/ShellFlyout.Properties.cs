// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace U5BFA.ShellFlyout
{
	public partial class ShellFlyout
	{
		public bool IsBackdropEnabled
		{
			get => (bool)GetValue(FlyoutHeightProperty);
			set => SetValue(FlyoutHeightProperty, value);
		}

		public static readonly DependencyProperty FlyoutHeightProperty =
			DependencyProperty.Register(
				nameof(IsBackdropEnabled),
				typeof(bool),
				typeof(ShellFlyout),
				new PropertyMetadata(true, (s, e) => ((ShellFlyout)s).OnIsBackdropEnabledChanged()));

		public Orientation PopupOrientation
		{
			get => (Orientation)GetValue(PopupOrientationProperty);
			set => SetValue(PopupOrientationProperty, value);
		}

		public static readonly DependencyProperty PopupOrientationProperty =
			DependencyProperty.Register(
				nameof(PopupOrientation),
				typeof(Orientation),
				typeof(ShellFlyout),
				new PropertyMetadata(Orientation.Vertical));

		public ContentBackdropManager? BackdropManager { get; set; }

		private void OnIsBackdropEnabledChanged()
		{
			TryToggleContentBackdropVisibility(IsBackdropEnabled);
		}

		private void OnContentChanged()
		{
			MainContentPresenter?.Content = Content;
		}

		private void OnCornerRadiusChanged()
		{
			UpdateBackdropTargetVisualClip();
		}
	}
}
