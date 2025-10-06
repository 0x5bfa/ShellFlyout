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

		public Orientation PopupDirection
		{
			get => (Orientation)GetValue(PopupDirectionProperty);
			set => SetValue(PopupDirectionProperty, value);
		}

		public static readonly DependencyProperty PopupDirectionProperty =
			DependencyProperty.Register(
				nameof(PopupDirection),
				typeof(Orientation),
				typeof(ShellFlyout),
				new PropertyMetadata(Orientation.Vertical));

		public MenuFlyout? MenuFlyout
		{
			get => (MenuFlyout)GetValue(MenuFlyoutProperty);
			set => SetValue(MenuFlyoutProperty, value);
		}

		public static readonly DependencyProperty MenuFlyoutProperty =
			DependencyProperty.Register(
				nameof(MenuFlyout),
				typeof(MenuFlyout),
				typeof(ShellFlyout),
				new PropertyMetadata(null));

		public bool IsTransitionAnimationEnabled
		{
			get => (bool)GetValue(IsTransitionAnimationEnabledProperty);
			set => SetValue(IsTransitionAnimationEnabledProperty, value);
		}

		public static readonly DependencyProperty IsTransitionAnimationEnabledProperty =
			DependencyProperty.Register(
				nameof(IsTransitionAnimationEnabled),
				typeof(bool),
				typeof(ShellFlyout),
				new PropertyMetadata(true));

		public ContentBackdropManager? BackdropManager { get; set; }

		private void OnIsBackdropEnabledChanged()
		{
			if (IsBackdropEnabled)
				EnsureContentBackdrop();
			else
				DiscardContentBackdrop();
		}

		private void OnContentChanged()
		{
			if (Content is not FrameworkElement newContentAsFrameworkElement)
				return;

			if (MainContentPresenter?.Content is FrameworkElement oldContentAsFrameworkElement)
				oldContentAsFrameworkElement.SizeChanged -= Content_SizeChanged;

			newContentAsFrameworkElement.SizeChanged += Content_SizeChanged;
			MainContentPresenter?.Content = Content;
		}

		private void OnCornerRadiusChanged()
		{
			UpdateBackdropVisual();
		}
	}
}
