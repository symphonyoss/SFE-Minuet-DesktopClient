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