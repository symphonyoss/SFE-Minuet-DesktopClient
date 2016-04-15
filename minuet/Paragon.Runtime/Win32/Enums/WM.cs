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

using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    public enum WM : uint
    {
        CREATE = 0x0001,
        DESTROY = 0x0002,
        MOVE = 0x0003,
        SIZE = 0x0005,
        ACTIVATE = 0x0006,
        SETFOCUS = 0x0007,
        KILLFOCUS = 0x0008,
        CLOSE = 0x0010,
        INITMENUPOPUP = 0x0117,
        ENTERSIZEMOVE = 0x0231,
        EXITSIZEMOVE = 0x0232,
        GETMINMAXINFO = 0x0024,
        GETTEXT = 0x000D,
        GETTEXTLENGTH = 0x000E,
        NCHITTEST = 0x0084,
        SIZING = 0x0214,
        MOVING = 0x0216,
        LBUTTONUP = 0x0202,
        LBUTTONDOWN = 0x0201,
        KEYDOWN = 0x0100,
        KEYUP = 0x0101,
        MOUSEACTIVATE = 0x0021,
        MOUSEMOVE = 0x0200,
        NCLBUTTONUP = 0x00A2,
        NCLBUTTONDOWN = 0x00A1,
        SYSCOMMAND = 0x0112,
        WINDOWPOSCHANGING = 0x0046,
        WINDOWPOSCHANGED = 0x0047,
        PARENTNOTIFY = 0x0210,
        HOTKEY = 0x0312
    }
}