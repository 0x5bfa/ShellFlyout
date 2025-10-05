// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

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

		private Grid? RootGrid;
		private Grid? SystemBackdropTargetGrid;
		private ContentPresenter? MainContentPresenter;

		public bool IsOpen { get; private set; }

		public bool IsAnimationPlaying { get; private set; }

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

			RegisterPropertyChangedCallback(ContentProperty, (s, e) => ((ShellFlyout)s).OnContentChanged());
			RegisterPropertyChangedCallback(CornerRadiusProperty, (s, e) => ((ShellFlyout)s).OnCornerRadiusChanged());

			//SystemBackdropTargetGrid.Loaded += SystemBackdropTargetGrid_Loaded;
			//SystemBackdropTargetGrid.Unloaded += SystemBackdropTargetGrid_Unloaded;

			if (Content is not null)
				MainContentPresenter?.Content = Content;
		}

		//private void SystemBackdropTargetGrid_Loaded(object sender, RoutedEventArgs e)
		//{
		//	Debug.WriteLine("SystemBackdropTargetGrid_Loaded");
		//}

		//private void SystemBackdropTargetGrid_Unloaded(object sender, RoutedEventArgs e)
		//{
		//	SystemBackdropTargetGrid?.Loaded -= SystemBackdropTargetGrid_Loaded;
		//	SystemBackdropTargetGrid?.Unloaded -= SystemBackdropTargetGrid_Unloaded;
		//}

		public async Task OpenFlyoutAsync()
		{
			if (_host?.DesktopWindowXamlSource is null)
				return;

			Debug.WriteLine("OpenFlyout in");

			var width = ((FrameworkElement)Content).Width + Margin.Left + Margin.Right;

			var bottomRightPoint = WindowHelpers.GetBottomRightCornerPoint();
			_host?.Resize(new(
				bottomRightPoint.X - width * _host.DesktopWindowXamlSource.SiteBridge.SiteView.RasterizationScale,
				0,
				width * _host.DesktopWindowXamlSource.SiteBridge.SiteView.RasterizationScale,
				bottomRightPoint.Y));

			UpdateLayout();

			await Task.Delay(1);

			if (IsBackdropEnabled)
				EnsureContentBackdrop();
			else
				DiscardContentBackdrop();

			if (IsTransitionAnimationEnabled && RootGrid is not null)
			{
				var storyboard = PopupDirection is Orientation.Vertical
					? TransitionHelpers.GetWindows11BottomToTopTransitionStoryboard(RootGrid, 760, 0)
					: TransitionHelpers.GetWindows11RightToLeftTransitionStoryboard(RootGrid, 384, 0);
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
					? TransitionHelpers.GetWindows11TopToBottomTransitionStoryboard(RootGrid, 0, 760)
					: TransitionHelpers.GetWindows11LeftToRightTransitionStoryboard(RootGrid, 0, 384);
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
			UpdateBackdropTargetVisualClip();
		}

		private void DiscardContentBackdrop()
		{
			if (_backdropLink is null)
				return;

			BackdropManager?.RemoveLink(_backdropLink);
			_backdropLink = null;
			_isBackdropLinkInitialized = false;
		}

		private void UpdateBackdropTargetVisualClip()
		{
			if (SystemBackdropTargetGrid is null || _backdropLink is null || SystemBackdropTargetGrid.ActualSize.X is 0 || SystemBackdropTargetGrid.ActualSize.Y is 0)
				return;

			_backdropLink.PlacementVisual.Size = SystemBackdropTargetGrid.ActualSize;
			_backdropLink.PlacementVisual.Clip = _backdropLink.PlacementVisual.Compositor.CreateRectangleClip(0, 0, SystemBackdropTargetGrid.ActualSize.X, SystemBackdropTargetGrid.ActualSize.Y,
				new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopLeft), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopLeft)),
				new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopRight), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopRight)),
				new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomRight), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomRight)),
				new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomLeft), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomLeft)));

			ElementCompositionPreview.SetElementChildVisual(SystemBackdropTargetGrid, _backdropLink.PlacementVisual);
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
