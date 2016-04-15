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

namespace Paragon.Plugins
{
    /// <summary>
    /// Defines the creation and management of windows for a single hosted or packaged application.
    /// </summary>
    /// <remarks>
    /// From https://developer.chrome.com/apps/app_lifecycle:
    /// An event page may create one or more windows at its discretion.
    /// By default, these windows are created with a script connection to the event page
    /// and are directly scriptable by the event page.
    ///
    /// Windows in Chrome Apps are not associated with any Chrome browser windows.
    /// They have an optional frame with title bar and size controls, and a recommended window ID.
    /// Windows without IDs will not restore to their size and location after restart.
    /// </remarks>
    public interface IApplicationWindowManager
    {
        IApplication Application { get; }
        IApplicationWindow[] AllWindows { get; }
        event EventHandler CreatingWindow;
        event Action<IApplicationWindow, bool> CreatedWindow;
        event EventHandler NoWindowsOpen;
    }
}