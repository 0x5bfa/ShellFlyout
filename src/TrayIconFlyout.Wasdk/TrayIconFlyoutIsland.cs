// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;

namespace U5BFA.TrayIconFlyout
{
	public partial class TrayIconFlyoutIsland : ContentControl
	{
		private const string PART_RootGrid = "PART_RootGrid";
		private const string PART_BackdropTargetGrid = "PART_BackdropTargetGrid";
		private const string PART_MainContentPresenter = "PART_MainContentPresenter";

		private WeakReference<TrayIconFlyout>? _owner;
		private ContentExternalBackdropLink? _backdropLink;
		private bool _isBackdropLinkAttached;
		private long _propertyChangedCallbackTokenForContentProperty;
		private long _propertyChangedCallbackTokenForCornerRadiusProperty;

		private Grid? RootGrid;
		private Grid? BackdropTargetGrid;
		private ContentPresenter? MainContentPresenter;

		public TrayIconFlyoutIsland()
		{
			DefaultStyleKey = typeof(TrayIconFlyoutIsland);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			RootGrid = GetTemplateChild(PART_RootGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_RootGrid} in the given {nameof(TrayIconFlyoutIsland)}'s style.");
			BackdropTargetGrid = GetTemplateChild(PART_BackdropTargetGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_BackdropTargetGrid} in the given {nameof(TrayIconFlyoutIsland)}'s style.");
			MainContentPresenter = GetTemplateChild(PART_MainContentPresenter) as ContentPresenter
				?? throw new MissingFieldException($"Could not find {PART_MainContentPresenter} in the given {nameof(TrayIconFlyoutIsland)}'s style.");

			_propertyChangedCallbackTokenForContentProperty = RegisterPropertyChangedCallback(ContentProperty, (s, e) => ((TrayIconFlyoutIsland)s).OnContentChanged());
			_propertyChangedCallbackTokenForCornerRadiusProperty = RegisterPropertyChangedCallback(CornerRadiusProperty, (s, e) => ((TrayIconFlyoutIsland)s).OnCornerRadiusChanged());

			Unloaded += TrayIconFlyoutIsland_Unloaded;
		}

		private void TrayIconFlyoutIsland_Unloaded(object sender, RoutedEventArgs e)
		{
			Unloaded -= TrayIconFlyoutIsland_Unloaded;

			UnregisterPropertyChangedCallback(ContentProperty, _propertyChangedCallbackTokenForContentProperty);
			UnregisterPropertyChangedCallback(CornerRadiusProperty, _propertyChangedCallbackTokenForCornerRadiusProperty);
		}

		internal void SetOwner(TrayIconFlyout owner)
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
				0, 0, 
				(float)MainContentPresenter.ActualWidth, 
				(float)MainContentPresenter.ActualHeight,
				new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopLeft - 1), Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopLeft - 1)),
				new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopRight - 1), Convert.ToSingle(BackdropTargetGrid.CornerRadius.TopRight - 1)),
				new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomRight - 1), Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomRight - 1)),
				new(Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomLeft - 1), Convert.ToSingle(BackdropTargetGrid.CornerRadius.BottomLeft - 1)));

			ElementCompositionPreview.SetElementChildVisual(BackdropTargetGrid, _backdropLink.PlacementVisual);
		}

		private void Content_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateBackdropVisual();
		}
	}
}
