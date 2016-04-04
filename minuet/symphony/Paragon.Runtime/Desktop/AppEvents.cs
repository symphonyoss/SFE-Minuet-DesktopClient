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

namespace Paragon.Runtime.Desktop
{
    /// <summary>
    /// Provides access to app related events.
    /// </summary>
    public static class AppEvents
    {
        /// <summary>
        /// Event fired when an app has exited.
        /// </summary>
        public static event EventHandler<AppEventArgs> AppExited;

        /// <summary>
        /// Event fired when a new instance of an app has been launched.
        /// </summary>
        public static event EventHandler<AppEventArgs> AppLaunched;

        internal static void RaiseAppExited(AppInfo appInfo)
        {
            RaiseAppEvent(appInfo, AppExited);
        }

        internal static void RaiseAppLaunched(AppInfo appInfo)
        {
            RaiseAppEvent(appInfo, AppLaunched);
        }

        private static void RaiseAppEvent(AppInfo appInfo, EventHandler<AppEventArgs> handler)
        {
            if (handler != null)
            {
                handler(null, new AppEventArgs(appInfo));
            }
        }
    }
}