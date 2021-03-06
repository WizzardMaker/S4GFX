﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

public static class BitmapToBitmapSource
{
	public static BitmapSource ToBitmapSource(this Bitmap source) {
		try {
			using (var handle = new SafeHBitmapHandle(source)) {
				return /*System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					   source.GetHbitmap(Color.Red),
					   IntPtr.Zero,
					   System.Windows.Int32Rect.Empty,
					   BitmapSizeOptions.FromWidthAndHeight(source.Width, source.Height));*/
				Imaging.CreateBitmapSourceFromHBitmap(handle.DangerousGetHandle(),
				IntPtr.Zero, Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());
			}
		}catch(Exception e) {
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);

			return null;
		}
	}

	[DllImport("gdi32")]
	private static extern int DeleteObject(IntPtr o);

	private sealed class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		[SecurityCritical]
		public SafeHBitmapHandle(Bitmap bitmap)
			: base(true) {
			SetHandle(bitmap.GetHbitmap());
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		protected override bool ReleaseHandle() {
			return DeleteObject(handle) > 0;
		}
	}
}