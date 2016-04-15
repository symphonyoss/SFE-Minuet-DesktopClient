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

namespace Paragon.Runtime
{
    internal static class WindowsVersion
    {
        private static bool? _isWin7OrNewer;

        public static bool IsWin7OrNewer
        {
            get
            {
                if (_isWin7OrNewer.HasValue)
                {
                    return _isWin7OrNewer.Value;
                }

                var major = Environment.OSVersion.Version.Major;
                var minor = Environment.OSVersion.Version.Minor;

                // Win7 (and Windows Server 2008 R2, the server equivalent) starts from 6.1.
                if (major < 6)
                {
                    // Major version is less than 5 -> not Win7.
                    _isWin7OrNewer = false;
                }
                else if (major == 6)
                {
                    // Major version is 6. If the minor version is at least 1 then 
                    // we are on Win7 or a later OS.
                    _isWin7OrNewer = minor >= 1;
                }
                else
                {
                    // Major version is greater than 6. We are on a Win7 or later OS.
                    _isWin7OrNewer = true;
                }

                return _isWin7OrNewer.Value;
            }
        }
    }
}