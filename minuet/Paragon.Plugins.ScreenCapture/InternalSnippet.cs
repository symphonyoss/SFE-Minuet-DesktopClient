//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

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