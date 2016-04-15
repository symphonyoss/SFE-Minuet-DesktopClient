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

using System.Runtime.InteropServices;
using System.Security.Permissions;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Desktop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    internal struct AppWindowProperties
    {
        private const string Prefix = "PGN_WND:";

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 37)]
        public string WorkspaceId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 37)]
        public string AppInstanceId;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool TryParse(string value, out AppWindowProperties props)
        {
            props = default(AppWindowProperties);
            if (string.IsNullOrEmpty(value)
                || !value.StartsWith(Prefix))
            {
                return false;
            }

            props = StructConverter.FromString<AppWindowProperties>(value.Substring(8));
            return true;
        }

        public override string ToString()
        {
            return string.Concat(Prefix, StructConverter.ToString(this));
        }
    }
}