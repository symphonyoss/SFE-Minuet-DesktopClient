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

using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Paragon.Plugins.Notifications
{
    public class Monitor : IMonitor
    {
        private readonly Screen screen;

        internal Monitor(Screen screen)
        {
            this.screen = screen;
        }

        public static IMonitor Primary
        {
            get { return new Monitor(Screen.PrimaryScreen); }
        }

        public Rect DeviceBounds
        {
            get { return GetRect(screen.Bounds); }
        }

        public Rect WorkingArea
        {
            get { return GetRect(screen.WorkingArea); }
        }

        public bool IsPrimary
        {
            get { return screen.Primary; }
        }

        public string DeviceName
        {
            get { return screen.DeviceName; }
        }

        private Rect GetRect(Rectangle value)
        {
            // should x, y, width, height be device-independent-pixels ??
            return new Rect
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
            };
        }
    }
}