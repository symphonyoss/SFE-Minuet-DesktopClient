using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Symphony.Plugins.MediaStreamPicker
{
    public class EnumScreenResult
    {
        public EnumScreenResult(uint _id, string _title, BitmapSource _image) 
        {
            id = _id;
            title = _title;
            image = _image;
        }

        public uint id { get; private set; }
        public string title { get; private set; }
        public BitmapSource image { get; private set; }
    }

    public class EnumerateScreens
    {

        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        public static IList<EnumScreenResult> getScreens()
        {
            IList<EnumScreenResult> results = new List<EnumScreenResult>();

            Screen[] screens = Screen.AllScreens;

            uint id = 0;
            foreach (Screen screen in screens)
            {
                var bmpScreenshot = new Bitmap(screen.Bounds.Width,
                                               screen.Bounds.Height,
                                               PixelFormat.Format32bppArgb);

                var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y,
                                            0, 0, screen.Bounds.Size,
                                           CopyPixelOperation.SourceCopy);

                BitmapSource bitmapSrc = EnumerateWindows.ToBitmapSource(bmpScreenshot);
                
                string title = screen.Primary ? "Primary Screen" : "Screen " + (id + 1);
                
                results.Add(new EnumScreenResult(id, title, bitmapSrc));
             }

            //IList<numScreenResult> results = getNumScreens();

            //if (results != null)
            //{
            //    int num = 0;
            //    foreach (numScreenResult result in results)
            //    {
            //        num++;
            //        string title = result.isPrimary ? "Primary Screen" : "Screen " + num;
            //        BitmapSource img = null;
            //        screens.Add(new EnumScreensResult(result.id, title, img));
            //    }
            //}

            return results;
        }

        class numScreenResult
        {
            public numScreenResult(uint _id, bool _isPrimary)
            {
                id = _id;
                isPrimary = _isPrimary;
            }

            public uint id { get; private set; }
            public bool isPrimary { get; private set; }
        }

        static List<numScreenResult> getNumScreens()
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            List<numScreenResult> deviceIds = new List<numScreenResult>();
            try
            {
                for (uint id = 0; ; id++)
                {
                    bool result = EnumDisplayDevices(null, id, ref d, 0);
                    if (result == false)
                        return deviceIds;

                    if ((d.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0)
                        continue;

                    if ((d.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) != 0)
                        deviceIds.Add(new numScreenResult(id, true));
                    else
                        deviceIds.Add(new numScreenResult(id, false));

                    d.cb = Marshal.SizeOf(d);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
