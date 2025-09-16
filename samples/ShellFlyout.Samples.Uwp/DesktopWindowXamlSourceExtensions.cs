using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Hosting;
using Windows.Win32;

namespace Terat
{
	public unsafe static class DesktopWindowXamlSourceExtensions
	{
		public static void Initialize(this DesktopWindowXamlSource @this, )
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
