using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Paragon.Plugins.ScreenCapture
{
    internal class InternalSnippet
    {
        public InternalSnippet(Image image, Rectangle selectedRectangle)
        {
            Image = image;
            SelectedRectangle = selectedRectangle;
        }

        /// <summary>
        /// The snipped screen image.
        /// </summary>
        public Image Image { get; private set; }

        /// <summary>
        /// Rectangle of the screen region that was snipped.
        /// </summary>
        public Rectangle SelectedRectangle { get; private set; }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr value);

        /// <summary>
        /// Convert the Image to a BitmapSource.
        /// </summary>
        /// <returns></returns>
        public BitmapSource ImageToBitmapSource()
        {
            var bitmap = new Bitmap(Image);
            var bmpPt = bitmap.GetHbitmap();
            var bitmapSource =
                Imaging.CreateBitmapSourceFromHBitmap(
                    bmpPt,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }
    }
}