// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

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
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RootGrid = GetTemplateChild(PART_RootGrid) as Grid
                ?? throw new MissingFieldException($"Could not find {PART_RootGrid} in the given {nameof(TrayIconFlyout)}'s style.");
            MenuFlyoutTargetControl = GetTemplateChild(PART_MenuFlyoutTargetControl) as Border
                ?? throw new MissingFieldException($"Could not find {PART_MenuFlyoutTargetControl} in the given {nameof(TrayIconFlyout)}'s style.");

            RootGrid.Background = new SolidColorBrush(Colors.Red);
        }

        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);

            _menuFlyout ??= new MenuFlyout();
            _menuFlyout.Items.Clear();

            foreach (var item in Items)
                _menuFlyout.Items.Add((MenuFlyoutItemBase)item);
        }

        public unsafe void Show(Point point)
        {
            if (_menuFlyout is null)
                return;

            UpdateFlyoutTheme();

            _host?.MoveAndResize(new(point.X, point.Y, 0, 0));
            _host?.SetHWndRectRegion(new(0, 0, 1, 1));
            _host?.UpdateWindowVisibility(true);

            _menuFlyout.ShowAt(MenuFlyoutTargetControl);

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