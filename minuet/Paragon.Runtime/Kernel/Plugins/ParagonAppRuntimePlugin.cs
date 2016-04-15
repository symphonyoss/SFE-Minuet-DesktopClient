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
using System.Collections.Generic;
using System.Linq;
using Paragon.Plugins;
using Paragon.Runtime.Annotations;
using Paragon.Runtime.Desktop;

namespace Paragon.Runtime.Kernel.Plugins
{
    [JavaScriptPlugin(Name = "paragon.app.runtime")]
    public class ParagonAppRuntimePlugin : ParagonPlugin
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private string _initialProtocolUri;
        private bool _protocolHandlingEnabled;

        /// <summary>
        /// This javascript event is fired into to the Event Page of the application and is an indication that the application's package is loaded and 
        /// has started executing.
        /// </summary>
        [JavaScriptPluginMember(Name = "onLaunched"), UsedImplicitly]
        public event JavaScriptPluginCallback Launched;

        [JavaScriptPluginMember(Name = "onExiting"), UsedImplicitly]
        public event JavaScriptPluginCallback Exiting;

        [JavaScriptPluginMember(Name = "onProtocolInvoke"), UsedImplicitly]
        public event JavaScriptPluginCallback ProtocolInvoke;

        [JavaScriptPluginMember, UsedImplicitly]
        public Dictionary<string, object> GetArgs()
        {
            return Application.Args;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public IParagonAppInfo GetCurrentApp()
        {
            Logger.Debug("GetCurrentApp");
            return ParagonDesktop.FindApps(opts => opts.InstanceId = Application.Metadata.InstanceId).FirstOrDefault();
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public string GetAppVersion()
        {
            if (Application != null && Application.Metadata != null)
            {
                var package = Application.Metadata.GetApplicationPackage();

                if (package != null && package.Manifest != null)
                {
                    return package.Manifest.Version;
                }
            }
            return null;
        }

        [JavaScriptPluginMember(Name = "applogfile"), UsedImplicitly]
        public string GetCurrentApplicationLogFilePath()
        {
            return ParagonLogManager.CurrentAppLogFile;
        }

        [JavaScriptPluginMember(Name = "applogfiles"), UsedImplicitly]
        public string[] GetArchiveApplicationLogFilePaths()
        {
            return ParagonLogManager.GetAppLogFiles();
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void EnableProtocolHandling()
        {
            _protocolHandlingEnabled = true;
            if (!string.IsNullOrEmpty(_initialProtocolUri))
            {
                ProtocolInvoke.Raise(() => new object[] {_initialProtocolUri});
                _initialProtocolUri = null;
            }
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public string GetApplicationPackageLocation()
        {
            var currentAppStartData = Application.Metadata.StartData;
            if (currentAppStartData == null)
            {
                return string.Empty;
            }

            Logger.Debug("Application package location is '{0}'", currentAppStartData);
            return currentAppStartData;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Application.Launched += ApplicationOnLaunched;
            Application.Exiting += ApplicationOnExiting;
            Application.ProtocolInvoke += ApplicationOnProtocolInvoke;
        }

        private void ApplicationOnProtocolInvoke(object sender, ProtocolInvocationEventArgs e)
        {
            if (!_protocolHandlingEnabled)
            {
                _initialProtocolUri = e.Uri;
            }
            else
            {
                ProtocolInvoke.Raise(() => new object[] {e.Uri});
            }
        }

        private void ApplicationOnExiting(object sender, ApplicationExitingEventArgs e)
        {
            Exiting.Raise(() => new object[] {e.SessionEnding});
        }

        private void ApplicationOnLaunched(object sender, EventArgs e)
        {
            Launched.Raise();
        }
    }
}