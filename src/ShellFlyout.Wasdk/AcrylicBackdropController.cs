// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;

#pragma warning disable CS8305

namespace Terat
{
	public static class AcrylicBackdropController
	{
		private static DesktopAcrylicController? _desktopAcrylicController;
		private static SystemBackdropConfiguration? _systemBackdropConfiguration;
		private static Compositor? _compositor;
		private static volatile bool _isInitialized;
		private static readonly List<ContentExternalBackdropLink> _contentExternalBackdropLinkCollection = [];

		public static bool IsInitialized => _isInitialized;

		public static void Initialize(Window mainWindow)
		{
			if (_desktopAcrylicController is null &&
				DesktopAcrylicController.IsSupported() &&
				mainWindow is not null &&
				mainWindow.Content is FrameworkElement frameworkElement)
			{
				mainWindow.DispatcherQueue.EnsureSystemDispatcherQueue();
				_compositor = mainWindow.Compositor;
				_desktopAcrylicController = new();
				_systemBackdropConfiguration = new() { Theme = (SystemBackdropTheme)frameworkElement.ActualTheme };
				_desktopAcrylicController.SetSystemBackdropConfiguration(_systemBackdropConfiguration);
				_isInitialized = true;
			}
		}

		public static void UnInitialize()
		{
			if (_desktopAcrylicController is not null)
			{
				try
				{
					_compositor = null;
					_systemBackdropConfiguration = null;
					_desktopAcrylicController.RemoveAllSystemBackdropTargets();
					_desktopAcrylicController.Dispose();
					foreach (ContentExternalBackdropLink contentExternalBackdropLink in _contentExternalBackdropLinkCollection)
					{
						contentExternalBackdropLink.Dispose();
					}
					_contentExternalBackdropLinkCollection.Clear();
				}
				catch  { }

				_isInitialized = false;
			}
		}

		public static ContentExternalBackdropLink? Create()
		{
			if (IsInitialized && _desktopAcrylicController is not null && _compositor is not null)
			{
				ContentExternalBackdropLink contentExternalBackdropLink = ContentExternalBackdropLink.Create(_compositor);
				contentExternalBackdropLink.ExternalBackdropBorderMode = CompositionBorderMode.Soft;
				_contentExternalBackdropLinkCollection.Add(contentExternalBackdropLink);
				_desktopAcrylicController.AddSystemBackdropTarget(contentExternalBackdropLink);
				return contentExternalBackdropLink;
			}
			else
			{
				return null;
			}
		}

		public static void Remove(ContentExternalBackdropLink contentExternalBackdropLink)
		{
			if (_contentExternalBackdropLinkCollection.Contains(contentExternalBackdropLink))
			{
				try
				{
					_desktopAcrylicController?.RemoveSystemBackdropTarget(contentExternalBackdropLink);
					contentExternalBackdropLink.Dispose();
					_contentExternalBackdropLinkCollection.Remove(contentExternalBackdropLink);
				}
				catch (Exception e)
				{
					ExceptionAsVoidMarshaller.ConvertToUnmanaged(e);
				}
			}
		}

		public static void UpdateControlTheme(ElementTheme elementTheme)
		{
			if (IsInitialized && _desktopAcrylicController is not null && _systemBackdropConfiguration is not null)
			{
				_systemBackdropConfiguration.Theme = Enum.TryParse(Convert.ToString(elementTheme), out SystemBackdropTheme systemBackdropTheme) ? systemBackdropTheme : SystemBackdropTheme.Default;
			}
		}
	}
}
