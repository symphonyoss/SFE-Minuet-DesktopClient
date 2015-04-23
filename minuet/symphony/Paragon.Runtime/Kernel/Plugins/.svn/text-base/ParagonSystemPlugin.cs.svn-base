using System;
using Paragon.Plugins;
using Paragon.Runtime.Annotations;
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