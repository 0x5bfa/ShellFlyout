// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

			UpdateBackdropVisual();

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
			return BackdropManager?.CreateLink();
		}

		internal void DiscardBackdropLink(ContentExternalBackdropLink backdropLink)
		{
			BackdropManager?.RemoveLink(backdropLink);
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

		private void UpdateBackdropVisual()
		{
			foreach (var island in Islands)
			{
				if (island is null)
					continue;

				if (IsBackdropEnabled)
				{
					island.EnsureContentBackdrop();
				}
				else
				{
					island.DiscardContentBackdrop();
				}
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
			BackdropManager?.Dispose();
			_host?.WindowInactivated -= HostWindow_Inactivated;
			_host?.Dispose();
		}
	}
}
