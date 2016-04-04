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
using Paragon.Plugins;
using Paragon.Runtime.Annotations;
using Paragon.Runtime.Desktop;
using Paragon.Runtime.Kernel.Windowing;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Kernel.Plugins
{
    [JavaScriptPlugin(Name = "paragon.system")]
    public class ParagonSystemPlugin : ParagonPlugin
    {
        [JavaScriptPluginMember(Name = "onDisplaySettingsChanged")]
        public event JavaScriptPluginCallback DisplaySettingsChanged;

        [JavaScriptPluginMember, UsedImplicitly]
        public SystemMemoryInfo GetMemoryInfo()
        {
            return new SystemMemoryInfo();
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public SystemCpuInfo GetCpuInfo()
        {
            return new SystemCpuInfo();
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public ScreenInfo[] GetScreenInfo()
        {
            return DisplaySettings.GetScreenInfo();
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void CreateMemoryDump(ProcessDumpType processDumpType)
        {
            var appInfo = ParagonDesktop.GetAppInfo(Application.Metadata.InstanceId);
            MemoryDump.CreateMemoryDump(appInfo, processDumpType);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            DisplaySettings.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
            DisplaySettings.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }

        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            DisplaySettingsChanged.Raise();
        }
    }
}