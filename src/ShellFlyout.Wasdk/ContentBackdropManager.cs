// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

#pragma warning disable CS8305

namespace Terat
{
	public partial class ContentBackdropManager : IDisposable
	{
		private ISystemBackdropControllerWithTargets? _backdropController;
		private SystemBackdropConfiguration? _configuration;
		private Compositor? _compositor;
		private readonly List<ContentExternalBackdropLink> _linkCollection = [];

		public static ContentBackdropManager? Create(ISystemBackdropControllerWithTargets backdropController, Compositor compositor, ElementTheme elementTheme)
		{
			var configuration = new SystemBackdropConfiguration() { Theme = (SystemBackdropTheme)elementTheme };
			backdropController.SetSystemBackdropConfiguration(configuration);

			return DesktopAcrylicController.IsSupported()
				? new ContentBackdropManager()
				{
					_compositor = compositor,
					_backdropController = backdropController,
					_configuration = configuration,
				}
				: null;
		}

		public ContentExternalBackdropLink? CreateLink()
		{
			if (_backdropController is null || _compositor is null)
				return null;

			var link = ContentExternalBackdropLink.Create(_compositor);
			link.ExternalBackdropBorderMode = CompositionBorderMode.Soft;
			_linkCollection.Add(link);
			_backdropController.AddSystemBackdropTarget(link);
			return link;
		}

		public void RemoveLink(ContentExternalBackdropLink link)
		{
			if (!_linkCollection.Contains(link))
				return;

			try
			{
				_backdropController?.RemoveSystemBackdropTarget(link);
				link.Dispose();
				_linkCollection.Remove(link);
			}
			catch { }
		}

		public void UpdateTheme(ElementTheme elementTheme)
		{
			if (_configuration is null)
				return;

			_configuration.Theme = (SystemBackdropTheme)elementTheme;
		}

		public void Dispose()
		{
			try
			{
				_compositor = null;
				_configuration = null;
				_backdropController?.RemoveAllSystemBackdropTargets();
				_backdropController?.Dispose();

				foreach (ContentExternalBackdropLink contentExternalBackdropLink in _linkCollection)
					contentExternalBackdropLink.Dispose();

				_linkCollection.Clear();
			}
			catch { }
		}
	}
}
