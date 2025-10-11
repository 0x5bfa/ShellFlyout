// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace U5BFA.ShellFlyout
{
	public partial class ShellFlyout
	{
		private readonly ObservableCollection<ShellFlyoutIsland> _islands = [];
		public IList<ShellFlyoutIsland> Islands => _islands;

		[GeneratedDependencyProperty]
		public partial object? IslandsSource { get; set; }

		[GeneratedDependencyProperty(DefaultValue = true)]
		public partial bool IsBackdropEnabled { get; set; }

		[GeneratedDependencyProperty(DefaultValue = Orientation.Vertical)]
		public partial Orientation PopupDirection { get; set; }

		[GeneratedDependencyProperty(DefaultValue = Orientation.Vertical)]
		public partial Orientation IslandsOrientation { get; set; }

		[GeneratedDependencyProperty(DefaultValue = FlyoutPlacementMode.BottomEdgeAlignedRight)]
		public partial FlyoutPlacementMode Placement { get; set; }

		[GeneratedDependencyProperty]
		public partial MenuFlyout? MenuFlyout { get; set; }

		[GeneratedDependencyProperty(DefaultValue = true)]
		public partial bool IsTransitionAnimationEnabled { get; set; }

		public ContentBackdropManager? BackdropManager { get; set; }

		partial void OnIsBackdropEnabledChanged(bool newValue)
		{
			UpdateBackdropVisual();
		}

		partial void OnIslandsSourcePropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is not IEnumerable<ShellFlyoutIsland> newIslands)
				return;

			Islands.Clear();

			foreach (var island in newIslands)
			{
				Islands.Add(island);
			}

			UpdateIslands();
		}
	}
}
