using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Paragon.Plugins.Notifications
{
    public class Monitor : IMonitor
    {
        private readonly Screen screen;

        internal Monitor(Screen screen)
        {
            this.screen = screen;
        }

        public static IMonitor Primary
        {
            get { return new Monitor(Screen.PrimaryScreen); }
        }

        public Rect DeviceBounds
        {
            get { return GetRect(screen.Bounds); }
        }

        public Rect WorkingArea
        {
            get { return GetRect(screen.WorkingArea); }
        }

        public bool IsPrimary
        {
            get { return screen.Primary; }
        }

        public string DeviceName
        {
            get { return screen.DeviceName; }
        }

        private Rect GetRect(Rectangle value)
        {
            // should x, y, width, height be device-independent-pixels ??
            return new Rect
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
            };
        }
    }
}