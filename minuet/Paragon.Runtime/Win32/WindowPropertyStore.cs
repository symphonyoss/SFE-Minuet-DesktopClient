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
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Paragon.Plugins;

namespace Paragon.Runtime.Win32
{
    internal static class WindowPropertyStore
    {
        internal const string PropertyStoreIid = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();

        /// <summary>
        /// Remove the "Pin this program to taskbar" and launch options from the task bar icon right click menu.
        /// </summary>
        /// <param name="hwnd">The window to remove the options for</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void PreventTaskbarPinning(IntPtr hwnd)
        {
            try
            {
                SetBool(hwnd, PropertyKeys.PreventPinning, true);
            }
            catch (Exception e)
            {
                Logger.Error("Error removing 'pin this program to taskbar' option from right-click menu", e);
            }
        }

        /// <summary>
        /// Set the app ID for a given window. All windows with the same ID will be grouped together in the task bar.
        /// </summary>
        /// <param name="hwnd">The window to set the ID for</param>
        /// <param name="appId">The app ID</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool SetAppId(IntPtr hwnd, string appId)
        {
            try
            {
                if (hwnd == IntPtr.Zero)
                {
                    return false;
                }

                SetString(hwnd, PropertyKeys.AppId, appId);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("Error setting app ID for the specified window", e);
                return false;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool GetComment(IntPtr hwnd, out string comment)
        {
            comment = null;

            try
            {
                if (hwnd == IntPtr.Zero)
                {
                    return false;
                }

                comment = GetProperty(hwnd, PropertyKeys.Comment).GetStringValue();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("Error getting comment field for the specified window", e);
                return false;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool SetComment(IntPtr hwnd, string text)
        {
            try
            {
                if (hwnd == IntPtr.Zero)
                {
                    return false;
                }

                SetString(hwnd, PropertyKeys.Comment, text);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("Error setting comment field for the specified window", e);
                return false;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool GetAppId(IntPtr hwnd, out string appId)
        {
            appId = null;

            try
            {
                if (hwnd == IntPtr.Zero)
                {
                    return false;
                }

                appId = GetProperty(hwnd, PropertyKeys.AppId).GetStringValue();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("Error getting app ID for the specified window", e);
                return false;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private static void SetBool(IntPtr hwnd, PropertyKey key, bool value)
        {
            var pv = new PropVariant();
            pv.SetValue(value);
            SetProperty(hwnd, key, pv);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private static void SetString(IntPtr hwnd, PropertyKey key, string value)
        {
            var pv = new PropVariant();
            pv.SetValue(value);
            SetProperty(hwnd, key, pv);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private static PropVariant GetProperty(IntPtr hwnd, PropertyKey key)
        {
            IPropertyStore propStore = null;
            PropVariant pv;

            try
            {
                propStore = GetWindowPropertyStore(hwnd);
                propStore.GetValue(ref key, out pv);
            }
            catch
            {
                pv = new PropVariant();
            }
            finally
            {
                if (propStore != null)
                {
                    Marshal.ReleaseComObject(propStore);
                }
            }

            return pv;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private static void SetProperty(IntPtr hwnd, PropertyKey key, PropVariant value)
        {
            IPropertyStore propStore = null;

            try
            {
                propStore = GetWindowPropertyStore(hwnd);
                propStore.SetValue(ref key, value);
                propStore.Commit();
            }
            finally
            {
                if (propStore != null)
                {
                    Marshal.ReleaseComObject(propStore);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private static IPropertyStore GetWindowPropertyStore(IntPtr hwnd)
        {
            var guid = new Guid(PropertyStoreIid);
            IPropertyStore propStore;
            var rc = NativeMethods.SHGetPropertyStoreForWindow(hwnd, ref guid, out propStore);
            if (rc != 0)
            {
                throw Marshal.GetExceptionForHR(rc);
            }

            return propStore;
        }

        private static class PropertyKeys
        {
            public static PropertyKey AppId
            {
                get { return new PropertyKey {FormatId = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), PropertyId = 5}; }
            }

            public static PropertyKey Comment
            {
                get { return new PropertyKey {FormatId = new Guid("{B9B4B3FC-2B51-4A42-B5D8-324146AFCF25}"), PropertyId = 5}; }
            }

            public static PropertyKey PreventPinning
            {
                get { return new PropertyKey {FormatId = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), PropertyId = 9}; }
            }
        }
    }
}