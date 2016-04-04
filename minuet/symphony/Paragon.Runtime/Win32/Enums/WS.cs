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
    internal enum WS : uint
    {
        OVERLAPPED = 0x00000000,
        CAPTION = 0x00C00000,
        SYSMENU = 0x00080000,
        THICKFRAME = 0x00040000,
        MINIMIZEBOX = 0x00020000,
        MAXIMIZEBOX = 0x00010000,
        POPUP = 0x80000000,
        OVERLAPPEDWINDOW = (OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX)
    }

    [ExcludeFromCodeCoverage]
    [Flags]
    internal enum WSEX
    {
        TOPMOST = 0x00000008
    }
}