// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

#pragma warning disable CS8305

namespace Terat
{
	public partial class ShellFlyout : ContentControl, IClosableContentControl
	{
		private XamlIslandHostWindow? _host;
		private DesktopWindowXamlSource? _source;
		private ContentExternalBackdropLink? _contentExternalBackdropLink;
		private bool _isBackdropLinkInitialized;

		private Grid? SystemBackdropTargetGrid;

		public bool IsOpen { get; private set; }

		public ContentBackdropManager? BackdropManager { get; set; }

		public ShellFlyout()
		{
			DefaultStyleKey = typeof(ShellFlyout);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			SystemBackdropTargetGrid = GetTemplateChild("SystemBackdropTargetGrid") as Grid
				?? throw new MissingFieldException($"Could not find {"SystemBackdropTargetGrid"} in the given {nameof(ShellFlyout)}'s style.");

			SystemBackdropTargetGrid.Loaded += ShellFlyout_Loaded;
		}

		private async void ShellFlyout_Loaded(object sender, RoutedEventArgs e)
		{
			if (SystemBackdropTargetGrid is not null)
			{
				if (!_isBackdropLinkInitialized)
				{
					_contentExternalBackdropLink = BackdropManager?.CreateLink();
					_isBackdropLinkInitialized = true;
				}

				if (_contentExternalBackdropLink is not null)
				{
					_contentExternalBackdropLink.PlacementVisual.Size = SystemBackdropTargetGrid.ActualSize;
					_contentExternalBackdropLink.PlacementVisual.Clip = _contentExternalBackdropLink.PlacementVisual.Compositor.CreateRectangleClip(0, 0, SystemBackdropTargetGrid.ActualSize.X, SystemBackdropTargetGrid.ActualSize.Y,
						new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopLeft), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopLeft)),
						new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopRight), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.TopRight)),
						new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomRight), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomRight)),
						new Vector2(Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomLeft), Convert.ToSingle(SystemBackdropTargetGrid.CornerRadius.BottomLeft)));
					ElementCompositionPreview.SetElementChildVisual(SystemBackdropTargetGrid, _contentExternalBackdropLink.PlacementVisual);
				}
			}

			VisualStateManager.GoToState(this, "Visible", true);
			await Task.Delay(200);

			IsOpen = true;
		}

		public unsafe void OpenFlyout()
		{
			RECT rect;
			PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA, 0, &rect, 0);
			double desktopWidth = rect.right;
			double desktopHeight = rect.bottom;

			double flyoutWidth = (Width + Margin.Left + Margin.Right) * 1;
			double flyoutHeight = (Height + Margin.Top + Margin.Bottom) * 1;

			var x = desktopWidth - flyoutWidth;
			var y = desktopHeight - flyoutHeight;

			var position = new Rect() { X = x, Y = y, Width = flyoutWidth, Height = flyoutHeight };
			_host = new XamlIslandHostWindow() { Position = position };
			_host.WindowInactivated += HostWindow_Inactivated;
			_source = _host.Initialize();
			_source?.Content = this;

			IsOpen = true;
		}

		private async void HostWindow_Inactivated(object? sender, EventArgs e)
		{
			await CloseFlyoutAsync();
		}

		public async Task CloseFlyoutAsync()
		{
			VisualStateManager.GoToState(this, "Collapsed", true);
			await Task.Delay(200);

			try
			{
				_host?.Dispose();
				_host = null;
			}
			catch { }

			IsOpen = false;
		}
	}
}
