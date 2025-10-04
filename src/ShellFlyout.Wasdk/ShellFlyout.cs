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

#pragma warning disable CS8305

namespace U5BFA.ShellFlyout
{
	public partial class ShellFlyout : ContentControl, IDisposable
	{
		private XamlIslandHostWindow? _host;
		private ContentExternalBackdropLink? _backdropLink;
		private bool _isBackdropLinkInitialized;

		private Grid? SystemBackdropTargetGrid;

		public bool IsOpen { get; private set; }

		public bool IsAnimationPlaying { get; private set; }

		public ContentBackdropManager? BackdropManager { get; set; }

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

			SystemBackdropTargetGrid = GetTemplateChild("SystemBackdropTargetGrid") as Grid
				?? throw new MissingFieldException($"Could not find {"SystemBackdropTargetGrid"} in the given {nameof(ShellFlyout)}'s style.");

			SystemBackdropTargetGrid.Loaded += SystemBackdropTargetGrid_Loaded;
			SystemBackdropTargetGrid.Unloaded += SystemBackdropTargetGrid_Unloaded;
		}

		private void SystemBackdropTargetGrid_Loaded(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("SystemBackdropTargetGrid_Loaded");

			if (SystemBackdropTargetGrid is not null)
			{
				if (!_isBackdropLinkInitialized)
				{
					_backdropLink = BackdropManager?.CreateLink();
					_isBackdropLinkInitialized = true;
				}

				if (_backdropLink is not null)
				{
					_backdropLink.PlacementVisual.Size = SystemBackdropTargetGrid.ActualSize;
					_backdropLink.PlacementVisual.Clip = _backdropLink.PlacementVisual.Compositor.CreateRectangleClip(0, 0, SystemBackdropTargetGrid.ActualSize.X, SystemBackdropTargetGrid.ActualSize.Y,
						new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopLeft), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopLeft)),
						new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopRight), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopRight)),
						new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomRight), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomRight)),
						new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomLeft), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomLeft)));
					ElementCompositionPreview.SetElementChildVisual(SystemBackdropTargetGrid, _backdropLink.PlacementVisual);
				}
			}
		}

		private void SystemBackdropTargetGrid_Unloaded(object sender, RoutedEventArgs e)
		{
			SystemBackdropTargetGrid?.Loaded -= SystemBackdropTargetGrid_Loaded;
			SystemBackdropTargetGrid?.Unloaded -= SystemBackdropTargetGrid_Unloaded;
		}

		public async Task OpenFlyoutAsync()
		{
			Debug.WriteLine("OpenFlyout in");

			var bottomRightPoint = WindowHelpers.GetBottomRightCornerPoint();
			_host?.Resize(new(bottomRightPoint.X - Width, 0, Width, bottomRightPoint.Y));

			VisualStateManager.GoToState(this, "Visible", true);
			await Task.Delay(200);

			IsOpen = true;

			Debug.WriteLine("OpenFlyout out");
		}

		public async Task CloseFlyoutAsync()
		{
			Debug.WriteLine("CloseFlyoutAsync in");

			VisualStateManager.GoToState(this, "Collapsed", true);
			await Task.Delay(200);

			_host?.Resize(new(0, 0, 0, 0));

			IsOpen = false;

			Debug.WriteLine("CloseFlyoutAsync out");
		}

		private async void HostWindow_Inactivated(object? sender, EventArgs e)
		{
			Debug.WriteLine("HostWindow_Inactivated");

			//await CloseFlyoutAsync();
		}

		public void Dispose()
		{
			_host?.WindowInactivated -= HostWindow_Inactivated;
			_host?.Dispose();
		}
	}
}
