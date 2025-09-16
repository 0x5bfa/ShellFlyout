// Copyright (c) Files Community
// Licensed under the MIT License.

global using static global::Windows.Win32.ManualDefinitions;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Windows.Win32
{
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate LRESULT WNDPROC([In] HWND hWnd, [In] uint uMsg, [In] WPARAM wParam, [In] LPARAM lParam);

	public static class ManualDefinitions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SUCCEEDED(HRESULT hr)
		{
			return hr >= 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool FAILED(HRESULT hr)
		{
			return hr < 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort LOWORD(nint l)
		{
			return unchecked((ushort)(((nuint)(l)) & 0xffff));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ushort HIWORD(nint l)
		{
			return ((ushort)((((nuint)(l)) >> 16) & 0xffff));
		}
	}
}
