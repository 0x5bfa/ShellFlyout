// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System.Drawing;
using Windows.Win32;

namespace U5BFA.ShellFlyout
{
	public partial class TrayIconMenuFlyout : Control
	{
		private readonly XamlIslandHostWindow? _host;
		private readonly Grid _flyoutPlacementTarget;
		private readonly MenuFlyout? _menuFlyout;

		public bool IsOpen => _menuFlyout?.IsOpen ?? false;

		public TrayIconMenuFlyout(MenuFlyout menuFlyout)
		{
			_flyoutPlacementTarget = new Grid() { Background = new SolidColorBrush(Colors.Black) };
			_menuFlyout = menuFlyout;
			_menuFlyout.Opened += MenuFlyout_Opened;

			_host = new XamlIslandHostWindow();
			_host.Initialize(_flyoutPlacementTarget);
			_host.UpdateWindowVisibility(false);
			//_host.WindowInactivated += HostWindow_Inactivated;
		}

		private void MenuFlyout_Opened(object? sender, object e)
		{
			if (sender is not MenuFlyout menuFlyout)
				return;
		}

		public unsafe void Show()
		{
			if (_menuFlyout is null)
				return;

			if (_menuFlyout.IsOpen)
				_menuFlyout.Hide();

			UpdateFlyoutTheme();

			Point cursorPos;
			PInvoke.GetCursorPos(&cursorPos);

			_host?.MoveAndResize(new(cursorPos.X, cursorPos.Y - 20, 1, 1));
			_host?.SetHWndRectRegion(new(0, 0, 1, 1));
			_host?.UpdateWindowVisibility(true);

			var options = new FlyoutShowOptions()
			{
				Placement = FlyoutPlacementMode.Top,
			};

			_menuFlyout.ShowAt(_flyoutPlacementTarget, options);
		}

		public void Hide()
		{
			_menuFlyout?.Hide();
			_host?.UpdateWindowVisibility(false);
		}

		private void UpdateFlyoutTheme()
		{
		}
	}
}
