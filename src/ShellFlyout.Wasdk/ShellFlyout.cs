// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Win32;

namespace U5BFA.ShellFlyout
{
	public partial class ShellFlyout : ContentControl, IDisposable
	{
		private const string PART_RootGrid = "PART_RootGrid";
		private const string PART_SystemBackdropTargetGrid = "PART_SystemBackdropTargetGrid";
		private const string PART_MainContentPresenter = "PART_MainContentPresenter";

		private XamlIslandHostWindow? _host;
		private ContentExternalBackdropLink? _backdropLink;
		private bool _isBackdropLinkInitialized;
		private long _propertyChangedCallbackTokenForContentProperty;
		private long _propertyChangedCallbackTokenForCornerRadiusProperty;

		private Grid? RootGrid;
		private Grid? SystemBackdropTargetGrid;
		private ContentPresenter? MainContentPresenter;

		public bool IsOpen { get; private set; }

		public bool IsAnimationPlaying { get; private set; }

		public double SiteViewRasterizationScale => _host?.DesktopWindowXamlSource?.SiteBridge.SiteView.RasterizationScale ?? RasterizationScale;

		public event EventHandler? Inactivated;

		public ShellFlyout()
		{
			DefaultStyleKey = typeof(ShellFlyout);

			VerticalAlignment = VerticalAlignment.Bottom;

			_host = new XamlIslandHostWindow();
			_host.Initialize(this);
			_host.WindowInactivated += HostWindow_Inactivated;
		}

		protected override void OnApplyTemplate()
		{
			Debug.WriteLine("OnApplyTemplate");

			base.OnApplyTemplate();

			RootGrid = GetTemplateChild(PART_RootGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_RootGrid} in the given {nameof(ShellFlyout)}'s style.");
			SystemBackdropTargetGrid = GetTemplateChild(PART_SystemBackdropTargetGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_SystemBackdropTargetGrid} in the given {nameof(ShellFlyout)}'s style.");
			MainContentPresenter = GetTemplateChild(PART_MainContentPresenter) as ContentPresenter
				?? throw new MissingFieldException($"Could not find {PART_MainContentPresenter} in the given {nameof(ShellFlyout)}'s style.");

			_propertyChangedCallbackTokenForContentProperty = RegisterPropertyChangedCallback(ContentProperty, (s, e) => ((ShellFlyout)s).OnContentChanged());
			_propertyChangedCallbackTokenForCornerRadiusProperty = RegisterPropertyChangedCallback(CornerRadiusProperty, (s, e) => ((ShellFlyout)s).OnCornerRadiusChanged());

			Unloaded += ShellFlyout_Unloaded;

			OnContentChanged();
		}

		private void ShellFlyout_Unloaded(object sender, RoutedEventArgs e)
		{
			Unloaded -= ShellFlyout_Unloaded;

			UnregisterPropertyChangedCallback(ContentProperty, _propertyChangedCallbackTokenForContentProperty);
			UnregisterPropertyChangedCallback(CornerRadiusProperty, _propertyChangedCallbackTokenForCornerRadiusProperty);
		}

		public async Task OpenFlyoutAsync()
		{
			if (_host?.DesktopWindowXamlSource is null)
				return;

			Debug.WriteLine("OpenFlyout in");

			ResizeAndPositionWindowToFitFlyout(false);

			UpdateLayout();
			await Task.Delay(1);

			if (IsBackdropEnabled)
				EnsureContentBackdrop();
			else
				DiscardContentBackdrop();

			_host?.UpdateWindowVisibility(true);

			if (IsTransitionAnimationEnabled && RootGrid is not null)
			{
				var storyboard = PopupDirection is Orientation.Vertical
					? TransitionHelpers.GetWindows11BottomToTopTransitionStoryboard(RootGrid, (int)ActualHeight, 0)
					: TransitionHelpers.GetWindows11RightToLeftTransitionStoryboard(RootGrid, (int)ActualWidth, 0);
				storyboard.Begin();
				await Task.Delay(200);
			}

			IsOpen = true;

			Debug.WriteLine("OpenFlyout out");
		}

		public async Task CloseFlyoutAsync()
		{
			Debug.WriteLine("CloseFlyoutAsync in");

			if (IsTransitionAnimationEnabled && RootGrid is not null)
			{
				var storyboard = PopupDirection is Orientation.Vertical
					? TransitionHelpers.GetWindows11TopToBottomTransitionStoryboard(RootGrid, 0, (int)ActualHeight)
					: TransitionHelpers.GetWindows11LeftToRightTransitionStoryboard(RootGrid, 0, (int)ActualWidth);
				storyboard.Begin();
				await Task.Delay(200);
			}

			_host?.Minimize();

			IsOpen = false;

			Debug.WriteLine("CloseFlyoutAsync out");
		}

		private void EnsureContentBackdrop()
		{
			if (SystemBackdropTargetGrid is null || _isBackdropLinkInitialized)
				return;

			_backdropLink = BackdropManager?.CreateLink();
			_isBackdropLinkInitialized = true;
			UpdateBackdropVisual();
		}

		private void DiscardContentBackdrop()
		{
			if (_backdropLink is null)
				return;

			BackdropManager?.RemoveLink(_backdropLink);
			_backdropLink = null;
			_isBackdropLinkInitialized = false;
		}

		private void UpdateBackdropVisual()
		{
			if (SystemBackdropTargetGrid is null || _backdropLink is null)
				return;

			var requestedWidth = (float)((FrameworkElement)Content).Width;
			var requestedHeight = (float)((FrameworkElement)Content).Height;

			_backdropLink.PlacementVisual.Size = new() { X = requestedWidth, Y = requestedHeight };
			_backdropLink.PlacementVisual.Clip = _backdropLink.PlacementVisual.Compositor.CreateRectangleClip(0, 0, requestedWidth, requestedHeight,
				new(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopLeft), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopLeft)),
				new(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopRight), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopRight)),
				new(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomRight), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomRight)),
				new(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomLeft), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomLeft)));

			ElementCompositionPreview.SetElementChildVisual(SystemBackdropTargetGrid, _backdropLink.PlacementVisual);
		}

		private void Content_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ResizeAndPositionWindowToFitFlyout(true);
			UpdateBackdropVisual();
		}

		private void ResizeAndPositionWindowToFitFlyout(bool showImmediately)
		{
			var windowSize = new Rect()
			{
				Width = (((FrameworkElement)Content).Width + Margin.Left + Margin.Right) * SiteViewRasterizationScale,
				Height = (((FrameworkElement)Content).Height + Margin.Top + Margin.Bottom) * SiteViewRasterizationScale,
			};

			var bottomRightPoint = WindowHelpers.GetBottomRightCornerPoint();

			_host?.Resize(
				new(bottomRightPoint.X - windowSize.Width,
					bottomRightPoint.Y - windowSize.Height,
					windowSize.Width,
					windowSize.Height),
				showImmediately);
		}

		private async void HostWindow_Inactivated(object? sender, EventArgs e)
		{
			Debug.WriteLine("HostWindow_Inactivated");

			//await CloseFlyoutAsync();
		}

		public void Dispose()
		{
			if (_backdropLink is not null) BackdropManager?.RemoveLink(_backdropLink);
			_host?.WindowInactivated -= HostWindow_Inactivated;
			_host?.Dispose();
		}
	}
}
