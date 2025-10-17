// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Drawing;
using Windows.Win32;

namespace U5BFA.TrayIconFlyout
{
	[ContentProperty(Name = nameof(Items))]
	public partial class TrayIconMenuFlyout : ItemsControl
	{
		private const string PART_RootGrid = "PART_RootGrid";
		private const string PART_MenuFlyoutTargetControl = "PART_MenuFlyoutTargetControl";

		private readonly XamlIslandHostWindow? _host;
		private MenuFlyout? _menuFlyout;

		private Grid? RootGrid;
		private Border? MenuFlyoutTargetControl;

		public bool IsOpen { get; private set; }

		public TrayIconMenuFlyout()
		{
			DefaultStyleKey = typeof(TrayIconMenuFlyout);

			_host = new XamlIslandHostWindow();
			_host.Initialize(this);
			_host.UpdateWindowVisibility(false);
			//_host.WindowInactivated += HostWindow_Inactivated;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			RootGrid = GetTemplateChild(PART_RootGrid) as Grid
				?? throw new MissingFieldException($"Could not find {PART_RootGrid} in the given {nameof(TrayIconFlyout)}'s style.");
			MenuFlyoutTargetControl = GetTemplateChild(PART_MenuFlyoutTargetControl) as Border
				?? throw new MissingFieldException($"Could not find {PART_MenuFlyoutTargetControl} in the given {nameof(TrayIconFlyout)}'s style.");
		}

		protected override void OnItemsChanged(object e)
		{
			base.OnItemsChanged(e);

			_menuFlyout ??= new MenuFlyout();
			_menuFlyout.Items.Clear();

			foreach (var item in Items)
				_menuFlyout.Items.Add((MenuFlyoutItemBase)item);
		}

		public unsafe void Show()
		{
			if (_menuFlyout is null)
				return;

			UpdateFlyoutTheme();

			Point cursorPos;
			PInvoke.GetCursorPos(&cursorPos);

			_host?.MoveAndResize(new(cursorPos.X　- 20, cursorPos.Y - 20, 5, 5));
			_host?.SetHWndRectRegion(new(0, 0, 5, 5));
			_host?.UpdateWindowVisibility(true);

			//DispatcherQueue.TryEnqueue(() =>
			//{
			//	if (_menuFlyout.FindDescendant<MenuFlyoutPresenter>() is { } menuFlyoutPresenter)
			//	{
			//	}
			//});

			_menuFlyout.ShowAt(MenuFlyoutTargetControl, new FlyoutShowOptions() { Placement = FlyoutPlacementMode.Top });

			IsOpen = true;
		}

		public void Hide()
		{
			_host?.UpdateWindowVisibility(false);

			_menuFlyout?.Hide();

			IsOpen = false;
		}

		private void UpdateFlyoutTheme()
		{
			if (GeneralHelpers.IsTaskbarLight())
			{
				RequestedTheme = ElementTheme.Light;
			}
			else
			{
				RequestedTheme = ElementTheme.Dark;
			}
		}
	}
}
