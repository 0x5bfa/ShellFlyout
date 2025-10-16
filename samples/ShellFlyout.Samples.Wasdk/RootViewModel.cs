// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace U5BFA.ShellFlyout
{
	internal partial class RootViewModel : ObservableObject
	{
		[ObservableProperty]
		internal partial bool IsBackdropEnabled { get; set; }

		[ObservableProperty]
		internal partial bool HideOnLostFocus { get; set; }

		[ObservableProperty]
		internal partial int SelectedPopupDirectionIndex { get; set; }

		[ObservableProperty]
		internal partial int SelectedFlyoutPlacementIndex { get; set; }

		public Dictionary<Orientation, string> PopupDirections { get; private set; } = [];
		public Dictionary<FlyoutPlacementMode, string> FlyoutPlacements { get; private set; } = [];

		internal RootViewModel()
		{
			IsBackdropEnabled = true;
			HideOnLostFocus = true;

			PopupDirections.Add(Orientation.Vertical, "Vertical");
			PopupDirections.Add(Orientation.Horizontal, "Horizontal");
			SelectedPopupDirectionIndex = 0;

			FlyoutPlacements.Add(FlyoutPlacementMode.TopEdgeAlignedLeft, "Top left");
			FlyoutPlacements.Add(FlyoutPlacementMode.TopEdgeAlignedRight, "Top right");
			FlyoutPlacements.Add(FlyoutPlacementMode.BottomEdgeAlignedLeft, "Bottom left");
			FlyoutPlacements.Add(FlyoutPlacementMode.BottomEdgeAlignedRight, "Bottom right");
			SelectedFlyoutPlacementIndex = 3;
		}

		partial void OnIsBackdropEnabledChanged(bool value)
		{
			TrayIconManager.Default.TrayIconFlyout?.IsBackdropEnabled = value;
		}

		partial void OnHideOnLostFocusChanged(bool value)
		{
			TrayIconManager.Default.TrayIconFlyout?.HideOnLostFocus = value;
		}

		partial void OnSelectedPopupDirectionIndexChanged(int value)
		{
			TrayIconManager.Default.TrayIconFlyout?.PopupDirection = PopupDirections.ElementAt(value).Key;
		}

		partial void OnSelectedFlyoutPlacementIndexChanged(int value)
		{
			TrayIconManager.Default.TrayIconFlyout?.Placement = FlyoutPlacements.ElementAt(value).Key;
		}
	}
}
