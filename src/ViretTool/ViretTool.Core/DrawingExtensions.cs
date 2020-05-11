using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ViretTool.Core
{
    public static class DrawingExtensions
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// https://stackoverflow.com/a/6484754
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            //IntPtr hBitmap = bitmap.GetHbitmap();
            //BitmapImage retval;

            //try
            //{
            //    retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
            //                 hBitmap,
            //                 IntPtr.Zero,
            //                 Int32Rect.Empty,
            //                 BitmapSizeOptions.FromEmptyOptions());
            //}
            //finally
            //{
            //    DeleteObject(hBitmap);
            //}

            //return retval;

            //bitmap = (Bitmap)bitmap.Clone();
            var bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;

        }
    }
}
