using System;
using Paragon.Plugins;
using System.Runtime.InteropServices;

namespace Symphony.Plugins
{
    /* 
     * This plugin looks for keyboard or mouse activity anywhere in the OS and raises
     * event when activity occurs.
     */
    
    [JavaScriptPlugin(Name = "symphony.activityDetector", IsBrowserSide = true)]
    public class ActivityPlugin : IParagonPlugin
    {
        [JavaScriptPluginMember(Name = "onActivity")]
        public event JavaScriptPluginCallback onActivity;

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        System.Threading.Timer timer = null;
        double numOfSecondsWaited = 60;
        double checkIntervalInSecs = 1;
        bool activityOccurred = false;

        public void Initialize(IApplication application)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            timer = new System.Threading.Timer(onTimer, null, 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(checkIntervalInSecs));
        }

        void onTimer(object state)
        {
            if (hasActivityOccurred())
            {
                activityOccurred = true;
            }

            if (activityOccurred)
            {
                numOfSecondsWaited += checkIntervalInSecs;

                // raise event at most once per minute to avoid flooding appBridge
                if (numOfSecondsWaited < 60)
                    return;

                numOfSecondsWaited = 0;
                activityOccurred = false;

                var evnt = onActivity;
                if (evnt != null)
                    evnt();
            }
        }

        uint? currentInputTick = null;
        
        bool hasActivityOccurred()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;

                bool hasChanged = false;

                if (currentInputTick != null)
                    hasChanged = lastInputTick != currentInputTick;

                currentInputTick = lastInputTick;

                return hasChanged;
            }

            return false;
        }

        public void Shutdown()
        {
            timer.Dispose();
        }
    }
}
