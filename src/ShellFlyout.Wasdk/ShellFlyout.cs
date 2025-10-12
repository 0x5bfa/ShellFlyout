// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace U5BFA.ShellFlyout
{
	public partial class ShellFlyout : Control, IDisposable
	{
		private const string PART_RootGrid = "PART_RootGrid";
		private const string PART_IslandsGrid = "PART_IslandsGrid";

		private readonly XamlIslandHostWindow? _host;
		private ContentBackdropManager? _backdropManager;
		private bool _wasTaskbarDark;
		private bool _wasTaskbarColorPrevalence;

		private Grid? RootGrid;
		private Grid? IslandsGrid;

		public bool IsOpen { get; private set; }

		public event EventHandler? Inactivated;

		public ShellFlyout()
		{
			DefaultStyleKey = typeof(ShellFlyout);

			_host = new XamlIslandHostWindow();
			_host.Initialize(this);
			_host.UpdateWindowVisibility(false);
			_host.WindowInactivated += HostWindow_Inactivated;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			RootGrid = GetTemplateChild(PART_RootGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_RootGrid} in the given {nameof(ShellFlyout)}'s style.");
			IslandsGrid = GetTemplateChild(PART_IslandsGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_IslandsGrid} in the given {nameof(ShellFlyout)}'s style.");

			Unloaded += ShellFlyout_Unloaded;

			UpdateIslands();
		}

		public async Task OpenFlyoutAsync()
		{
			if (_host?.DesktopWindowXamlSource is null)
				return;

			_host.MaximizeHWnd();
			_host.MaximizeXamlIslandHWnd();

			UpdateFlyoutRegion();
			UpdateFlyoutTheme();
			UpdateBackdropManager();

			UpdateLayout();
			await Task.Delay(1);

			_host.UpdateWindowVisibility(true);

			if (IsTransitionAnimationEnabled && RootGrid is not null)
			{
				var storyboard = PopupDirection is Orientation.Vertical
					? TransitionHelpers.GetWindows11BottomToTopTransitionStoryboard(RootGrid, (int)ActualHeight, 0)
					: TransitionHelpers.GetWindows11RightToLeftTransitionStoryboard(RootGrid, (int)ActualWidth, 0);
				storyboard.Begin();
				await Task.Delay(200);
			}

			IsOpen = true;
		}

		public async Task CloseFlyoutAsync()
		{
			if (IsTransitionAnimationEnabled && RootGrid is not null)
			{
				var storyboard = PopupDirection is Orientation.Vertical
					? TransitionHelpers.GetWindows11TopToBottomTransitionStoryboard(RootGrid, 0, (int)ActualHeight)
					: TransitionHelpers.GetWindows11LeftToRightTransitionStoryboard(RootGrid, 0, (int)ActualWidth);
				storyboard.Begin();
				await Task.Delay(200);
			}

			_host?.UpdateWindowVisibility(false);

			IsOpen = false;
		}

		internal ContentExternalBackdropLink? CreateBackdropLink()
		{
			return _backdropManager?.CreateLink();
		}

		internal void DiscardBackdropLink(ContentExternalBackdropLink backdropLink)
		{
			_backdropManager?.RemoveLink(backdropLink);
		}

		private void UpdateBackdropManager()
		{
			var _isTaskbarDark = RegistryHelpers.IsTaskbarLight();
			var _isTaskbarColorPrevalence = RegistryHelpers.IsTaskbarColorPrevalenceEnabled();
			bool shouldUpdateBackdrop = _wasTaskbarDark != _isTaskbarDark || _wasTaskbarColorPrevalence != _isTaskbarColorPrevalence;
			_wasTaskbarDark = _isTaskbarDark;
			_wasTaskbarColorPrevalence = _isTaskbarColorPrevalence;
			if (!shouldUpdateBackdrop)
				return;

			var controller = BackdropController
				?? (_isTaskbarColorPrevalence
					? BackdropControllerHelpers.GetAccentedAcrylicController(Resources)
					: _isTaskbarDark
						? BackdropControllerHelpers.GetLightAcrylicController(Resources)
						: BackdropControllerHelpers.GetDarkAcrylicController(Resources));
			if (controller is null)
				return;

			_backdropManager?.Dispose();
			_backdropManager = null;
			_backdropManager = ContentBackdropManager.Create(controller, ElementCompositionPreview.GetElementVisual(IslandsGrid).Compositor, ActualTheme);

			UpdateBackdrop(true);
		}

		private void UpdateFlyoutTheme()
		{
			if (RegistryHelpers.IsTaskbarLight())
			{
				foreach (var island in Islands)
					island.RequestedTheme = ElementTheme.Light;
			}
			else
			{
				foreach (var island in Islands)
					island.RequestedTheme = ElementTheme.Dark;
			}
		}

		private void UpdateIslands()
		{
			if (IslandsGrid is null)
				return;

			IslandsGrid.Children.Clear();
			IslandsGrid.RowDefinitions.Clear();
			IslandsGrid.ColumnDefinitions.Clear();

			if (IslandsOrientation is Orientation.Vertical)
			{
				for (int index = 0; index < Islands.Count; index++)
				{
					if (Islands[index] is not ShellFlyoutIsland island)
						continue;

					IslandsGrid.RowDefinitions.Add(new() { Height = GridLength.Auto });
					Grid.SetRow(island, index);
					Grid.SetColumn(island, 0);
					island.SetOwner(this);
					IslandsGrid.Children.Add(island);
				}
			}
			else
			{
				for (int index = 0; index < Islands.Count; index++)
				{

					if (Islands[index] is not ShellFlyoutIsland island)
						continue;

					IslandsGrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
					Grid.SetRow(island, 0);
					Grid.SetColumn(island, index);
					island.SetOwner(this);
					IslandsGrid.Children.Add(island);
				}
			}
		}

		private void UpdateBackdrop(bool coerce = false)
		{
			foreach (var island in Islands)
			{
				island.UpdateBackdrop(IsBackdropEnabled, coerce);
			}
		}

		private void UpdateFlyoutRegion()
		{
			if (_host?.DesktopWindowXamlSource is null || IslandsGrid is null)
				return;

			var flyoutWidth = (IslandsGrid.ActualWidth + Margin.Left + Margin.Right) * RasterizationScale;
			var flyoutHeight = (IslandsGrid.ActualHeight + Margin.Top + Margin.Bottom) * RasterizationScale;

			_host?.SetHWndRectRegion(new(
				(int)(_host.WindowSize.Width - flyoutWidth),
				(int)(_host.WindowSize.Height - flyoutHeight),
				(int)_host.WindowSize.Width,
				(int)_host.WindowSize.Height));
		}

		private void ShellFlyout_Unloaded(object sender, RoutedEventArgs e)
		{
			Unloaded -= ShellFlyout_Unloaded;
		}

		private async void HostWindow_Inactivated(object? sender, EventArgs e)
		{
			Debug.WriteLine("HostWindow_Inactivated");

			//await CloseFlyoutAsync();
		}

		public void Dispose()
		{
			_backdropManager?.Dispose();
			_host?.WindowInactivated -= HostWindow_Inactivated;
			_host?.Dispose();
		}
	}
}
