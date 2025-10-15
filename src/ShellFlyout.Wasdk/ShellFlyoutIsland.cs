// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;

namespace U5BFA.ShellFlyout
{
	public partial class ShellFlyoutIsland : ContentControl
	{
		private const string PART_RootGrid = "PART_RootGrid";
		private const string PART_BackdropTargetGrid = "PART_BackdropTargetGrid";
		private const string PART_MainContentPresenter = "PART_MainContentPresenter";

		private readonly XamlIslandHostWindow? _host;

		private WeakReference<ShellFlyout>? _owner;
		private ContentExternalBackdropLink? _backdropLink;
		private bool _isBackdropLinkAttached;
		private long _propertyChangedCallbackTokenForContentProperty;
		private long _propertyChangedCallbackTokenForCornerRadiusProperty;

		private Grid? RootGrid;
		private Grid? BackdropTargetGrid;
		private ContentPresenter? MainContentPresenter;

		public ShellFlyoutIsland()
		{
			DefaultStyleKey = typeof(ShellFlyoutIsland);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			RootGrid = GetTemplateChild(PART_RootGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_RootGrid} in the given {nameof(ShellFlyoutIsland)}'s style.");
			BackdropTargetGrid = GetTemplateChild(PART_BackdropTargetGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_BackdropTargetGrid} in the given {nameof(ShellFlyoutIsland)}'s style.");
			MainContentPresenter = GetTemplateChild(PART_MainContentPresenter) as ContentPresenter
				?? throw new MissingFieldException($"Could not find {PART_MainContentPresenter} in the given {nameof(ShellFlyoutIsland)}'s style.");

			_propertyChangedCallbackTokenForContentProperty = RegisterPropertyChangedCallback(ContentProperty, (s, e) => ((ShellFlyoutIsland)s).OnContentChanged());
			_propertyChangedCallbackTokenForCornerRadiusProperty = RegisterPropertyChangedCallback(CornerRadiusProperty, (s, e) => ((ShellFlyoutIsland)s).OnCornerRadiusChanged());

			Unloaded += ShellFlyoutIsland_Unloaded;
		}

		private void ShellFlyoutIsland_Unloaded(object sender, RoutedEventArgs e)
		{
			Unloaded -= ShellFlyoutIsland_Unloaded;

			UnregisterPropertyChangedCallback(ContentProperty, _propertyChangedCallbackTokenForContentProperty);
			UnregisterPropertyChangedCallback(CornerRadiusProperty, _propertyChangedCallbackTokenForCornerRadiusProperty);
		}

		internal void SetOwner(ShellFlyout owner)
		{
			_owner = new(owner);
		}

		internal void UpdateBackdrop(bool isEnabled, bool coerce = false)
		{
			if (_owner is null || !_owner.TryGetTarget(out var owner) || owner.BackdropManager is null)
				return;

			if (isEnabled)
			{
				if (_isBackdropLinkAttached)
				{
					if (coerce)
					{
						if (_backdropLink is null)
							return;

						owner.BackdropManager.RemoveLink(_backdropLink);
						_backdropLink = null;
						_isBackdropLinkAttached = false;
					}
					else
					{
						return;
					}
				}

				_backdropLink = owner.BackdropManager.CreateLink();
				_isBackdropLinkAttached = true;
				UpdateBackdropVisual();
			}
			else
			{
				if (_backdropLink is null)
					return;

				owner.BackdropManager.RemoveLink(_backdropLink);
				_backdropLink = null;
				_isBackdropLinkAttached = false;
			}
		}

		internal void UpdateBackdropVisual()
		{
			if (BackdropTargetGrid is null || _backdropLink is null || Content is not FrameworkElement element)
				return;

			_backdropLink.PlacementVisual.Size = new((float)ActualWidth, (float)ActualHeight);
			_backdropLink.PlacementVisual.Clip = _backdropLink.PlacementVisual.Compositor.CreateRectangleClip(
				0, 0, (float)ActualWidth, (float)ActualHeight,
				new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopLeft), Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopLeft)),
				new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopRight), Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopRight)),
				new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomRight), Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomRight)),
				new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomLeft), Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomLeft)));

			ElementCompositionPreview.SetElementChildVisual(BackdropTargetGrid, _backdropLink.PlacementVisual);
		}

		private void UpdateFlyoutRegion()
		{
			if (_host?.DesktopWindowXamlSource is null)
				return;

			var flyoutWidth = (((FrameworkElement)Content).Width + Margin.Left + Margin.Right) * RasterizationScale;
			var flyoutHeight = (((FrameworkElement)Content).Height + Margin.Top + Margin.Bottom) * RasterizationScale;

			_host?.SetHWndRectRegion(new(
				(int)(_host.WindowSize.Width - flyoutWidth),
				(int)(_host.WindowSize.Height - flyoutHeight),
				(int)_host.WindowSize.Width,
				(int)_host.WindowSize.Height));
		}

		private void Content_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateFlyoutRegion();
			UpdateBackdropVisual();
		}
	}
}
