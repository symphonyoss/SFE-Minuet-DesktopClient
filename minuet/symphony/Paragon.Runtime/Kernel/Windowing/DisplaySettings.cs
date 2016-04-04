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
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Paragon.Runtime.Kernel.Windowing
{
    internal static class DisplaySettings
    {
        static DisplaySettings()
        {
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        public static event EventHandler DisplaySettingsChanged;

        public static ScreenInfo[] GetScreenInfo()
        {
            return Screen.AllScreens
                .Select(s => new ScreenInfo
                {
                    Name = s.DeviceName,
                    IsPrimary = s.Primary,
                    Bounds = Bounds.FromRectangle(s.Bounds),
                    WorkArea = Bounds.FromRectangle(s.WorkingArea),
                })
                .ToArray();
        }

        private static void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            var evnt = DisplaySettingsChanged;
            if (evnt != null)
            {
                evnt(sender, e);
            }
        }
    }
}