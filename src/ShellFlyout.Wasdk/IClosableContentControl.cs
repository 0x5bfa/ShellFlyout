// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Terat
{
	public interface IClosableContentControl
	{
		public bool IsOpen { get; }

		public Task CloseFlyoutAsync();
	}
}
