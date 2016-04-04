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
    internal enum FlashWindowFlags : uint
    {
        /// <summary>
        /// Stop flashing. The system restores the window to its original state.
        /// </summary>
        FLASHW_STOP = 0,

        /// <summary>
        /// Flash the window caption.
        /// </summary>
        FLASHW_CAPTION = 1,

        /// <summary>
        /// Flash the taskbar button.
        /// </summary>
        FLASHW_TRAY = 2,

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// </summary>
        FLASHW_ALL = 3,

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        FLASHW_TIMER = 4,

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        FLASHW_TIMERNOFG = 12
    }
}