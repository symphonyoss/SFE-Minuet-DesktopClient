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
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Paragon.Runtime.Win32;
using Image = System.Windows.Controls.Image;

namespace Paragon.Runtime.WPF
{
    public class IconImage
    {
        private readonly Lazy<Icon> _icon;
        private readonly Lazy<Image> _image;
        private readonly string _uri;

        public IconImage(string uri)
        {
            _uri = uri;
            _image = new Lazy<Image>(GetImage);
            _icon = new Lazy<Icon>(GetIcon);
        }

        // Used by the app window.
        public ImageSource ImageSource
        {
            get { return _image.Value.Source; }
        }

        // Used by the system tray.
        public Icon Icon
        {
            get { return _icon.Value; }
        }

        // Used by the app bar.
        public Visual Visual
        {
            get { return _image.Value; }
        }

        private Image GetImage()
        {
            if (_uri.StartsWith("pack"))
            {
                var icon = _icon.Value;

                using (var bmp = icon.ToBitmap())
                {
                    var hbmp = bmp.GetHbitmap();

                    try
                    {
                        var source = Imaging.CreateBitmapSourceFromHBitmap(
                            hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                        return new Image {Source = source};
                    }
                    finally
                    {
                        Win32Api.DeleteObject(hbmp);
                    }
                }
            }

            var decoder = BitmapDecoder.Create(
                new Uri(_uri),
                BitmapCreateOptions.DelayCreation,
                BitmapCacheOption.OnDemand);

            var result = decoder.Frames.FirstOrDefault(f => Math.Abs(f.Width - 32) < 0.1)
                         ?? decoder.Frames.OrderBy(f => f.Width).First();

            return new Image {Source = result};
        }

        private Icon GetIcon()
        {
            if (_uri.StartsWith("pack"))
            {
                var streamInfo = Application.GetResourceStream(new Uri(_uri));
                return streamInfo == null ? null : new Icon(streamInfo.Stream);
            }

            return new Icon(_uri);
        }
    }
}