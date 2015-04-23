using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Paragon.Runtime.Kernel.Windowing
{
    internal static class DisplaySettings
    {
        static DisplaySettings()
        {
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        public static event EventHandler DisplaySettingsChanged;

        public static ScreenInfo[] GetScreenInfo()
        {
            return Screen.AllScreens
                .Select(s => new ScreenInfo
                {
                    Name = s.DeviceName,
                    IsPrimary = s.Primary,
                    Bounds = Bounds.FromRectangle(s.Bounds),
                    WorkArea = Bounds.FromRectangle(s.WorkingArea),
                })
                .ToArray();
        }

        private static void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            var evnt = DisplaySettingsChanged;
            if (evnt != null)
            {
                evnt(sender, e);
            }
        }
    }
}