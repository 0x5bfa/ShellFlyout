// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.

using Windows.UI.Xaml.Hosting;
using Windows.Win32;

namespace U5BFA
{
	public unsafe static class DesktopWindowXamlSourceExtensions
	{
		public static void Initialize(this DesktopWindowXamlSource @this)
		{
			// Is this needed anymore? maybe for older builds?
			fixed (char* pwszTwinApiAppCoreDll = "twinapi.appcore.dll", pwszThreadPoolWinRTDll = "threadpoolwinrt.dll")
			{
				PInvoke.LoadLibrary(pwszTwinApiAppCoreDll);
				PInvoke.LoadLibrary(pwszThreadPoolWinRTDll);
			}

			WindowsXamlManager.InitializeForCurrentThread();
		}
	}
}
