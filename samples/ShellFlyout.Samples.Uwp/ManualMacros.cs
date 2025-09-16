// Copyright (c) Files Community
// Licensed under the MIT License.

global using static global::Windows.Win32.ManualMacros;

using Windows.Win32.Foundation;

namespace Windows.Win32
{
	public static class ManualMacros
	{
		public static bool SUCCEEDED(HRESULT hr)
		{
			return hr >= 0;
		}

		public static bool FAILED(HRESULT hr)
		{
			return hr < 0;
		}

		public static ushort LOWORD(nint l)
		{
			return unchecked((ushort)(((nuint)(l)) & 0xffff));
		}

		public static ushort HIWORD(nint l)
		{
			return ((ushort)((((nuint)(l)) >> 16) & 0xffff));
		}
	}
}
