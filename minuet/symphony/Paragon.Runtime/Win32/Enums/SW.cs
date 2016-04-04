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
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [Flags]
    public enum SW : uint
    {
        FORCEMINIMIZE = 0x0000000B,
        HIDE = 0x00000000,
        MAXIMIZE = 0x00000003,
        MINIMIZE = 0x00000006,
        RESTORE = 0x00000009,
        SHOW = 0x00000005,
        SHOWDEFAULT = 0x0000000A,
        SHOWMAXIMIZED = 0x00000003,
        SHOWMINIMIZED = 0x00000002,
        SHOWMINNOACTIVE = 0x00000007,
        SHOWNA = 0x00000008,
        SHOWNOACTIVATE = 0x00000004,
        SHOWNORMAL = 0x00000001
    }
}