// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace U5BFA
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		public App(XamlIslandWindow host)
		{
			var source = host.InitializeXamlIsland();

			SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));

			var frame = new Frame();
			source.Content = frame;
			frame.Margin = new(12);
			frame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
		}
	}
}
