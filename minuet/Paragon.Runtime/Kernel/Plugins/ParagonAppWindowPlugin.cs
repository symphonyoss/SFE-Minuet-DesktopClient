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

using System.Linq;
using Paragon.Plugins;
using Paragon.Runtime.Annotations;
using Paragon.Runtime.Kernel.Windowing;
using Microsoft.Win32;
using System;

namespace Paragon.Runtime.Kernel.Plugins
{
    /// <summary>
    /// This class implements the paragon.app.window part of the Kernel.
    /// Should be instantiated once per application. Creates related browser windows and co-ordinates them being in workspaces.
    /// </summary>
    [JavaScriptPlugin(Name = "paragon.app.window", IsBrowserSide = true)]
    public class ParagonAppWindowPlugin : ParagonPlugin
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();

        /// <summary>
        /// The size and position of a window can be specified in a number of different ways.
        /// The most simple option is not specifying anything at all, in which case a default size and platform dependent position will be used.
        /// 
        /// To set the position, size and constraints of the window, use the innerBounds or outerBounds properties.
        /// Inner bounds do not include window decorations. Outer bounds include the window's title bar and frame.
        /// Note that the padding between the inner and outer bounds is determined by the OS.
        /// Therefore setting the same property for both inner and outer bounds is considered an error 
        /// (for example, setting both innerBounds.left and outerBounds.left).
        /// 
        /// To automatically remember the positions of windows you can give them ids.
        /// If a window has an id, This id is used to remember the size and position of the window whenever it is moved or resized.
        /// This size and position is then used instead of the specified bounds on subsequent opening of a window with the same id.
        /// If you need to open a window with an id at a location other than the remembered default, you can create it hidden, 
        /// move it to the desired location, then show it.
        /// </summary>
        /// <param name="startUrl"></param>
        /// <param name="options">
        /// OPTIONAL.
        /// </param>
        /// <param name="callback">
        /// OPTIONAL.
        /// 
        /// Called in the creating window (parent) before the load event is called in the created window (child).
        /// The parent can set fields or functions on the child usable from onload. E.g. background.js:
        /// 
        /// function(createdWindow) { createdWindow.contentWindow.foo = function () { }; };
        /// 
        /// window.js:
        /// 
        /// window.onload = function () { foo(); }
        /// 
        /// If you specify the callback parameter, it should be a function that looks like this:
        /// 
        /// function(AppWindow createdWindow) {...};
        /// </param>
        [JavaScriptPluginMember, UsedImplicitly]
        public void Create(string startUrl, CreateWindowOptions options, JavaScriptPluginCallback callback)
        {
            String podUrl = "";

            using (RegistryKey symphony = Registry.ClassesRoot.OpenSubKey("symphony"))
            {
                if (symphony != null && !String.IsNullOrEmpty(podUrl))
                {
                    podUrl = (string)symphony.GetValue("PodUrl", "");
                    Uri uri;
                    if (Uri.TryCreate(podUrl, UriKind.Absolute, out uri) && uri.Scheme == Uri.UriSchemeHttps)
                    {
                        startUrl = uri.ToString();
                        Logger.Info(string.Format("PodUrl at Registry key : {0}", startUrl));
                    }
                }            

            }

            Logger.Info(string.Format("Create window : {0}", startUrl));
            var windowManager = Application.WindowManager as IApplicationWindowManagerEx;
            if (windowManager != null)
            {
                windowManager.CreateWindow(new CreateWindowRequest(startUrl, options, callback));
            }
        }

        /// <summary>
        /// Returns an AppWindow object for the current script context (ie JavaScript 'window' object). 
        /// This can also be called on a handle to a script context for another page, for example: otherWindow.chrome.app.window.current().
        /// </summary>
        /// <returns></returns>
        [JavaScriptPluginMember, UsedImplicitly]
        public IApplicationWindow GetCurrent()
        {
            Logger.Debug("GetCurrent");
            var windowManager = Application.WindowManager;
            if (windowManager != null)
            {
                var browserId = PluginExecutionContext.BrowserIdentifier;
                return windowManager.AllWindows.FirstOrDefault(window => window.ContainsBrowser(browserId));
            }
            return null;
        }

        /// <summary>
        /// Gets an array of all currently created app windows.
        /// </summary>
        /// <returns></returns>
        [JavaScriptPluginMember, UsedImplicitly]
        public IApplicationWindow[] GetAll()
        {
            Logger.Debug("GetAll");
            var windowManager = Application.WindowManager;
            return windowManager != null ? windowManager.AllWindows : new IApplicationWindow[0];
        }

        /// <summary>
        /// Gets an AppWindow with the given id. If no window with the given id exists null is returned.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [JavaScriptPluginMember, UsedImplicitly]
        public IApplicationWindow GetById(string id)
        {
            Logger.Debug("GetById");
            var windowManager = Application.WindowManager;
            return windowManager != null ? windowManager.AllWindows.FirstOrDefault(window => window.GetId() == id) : null;
        }
    }
}