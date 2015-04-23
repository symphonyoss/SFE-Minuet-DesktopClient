using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace Symphony.NativeServices
{
    public class TelemetryNativeService
    {
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;

        private string windowDpi;

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        public JObject GetClientInfo()
        {
            JObject json = new JObject();

            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    json["ip"] = ip.ToString();
                    break;
                }
            }

            json["version"] = "LiveCurrentClient_" + Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version;

            json["windowDpi"] = this.GetWindowDpi();

            Trace.TraceInformation("ClientInfo: " + json);

            return json;
        }

        private string GetWindowDpi()
        {
            if (this.windowDpi == null)
            {
                int xDpi;
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr desktop = g.GetHdc();
                    xDpi = GetDeviceCaps(desktop, LOGPIXELSX);
                }
                
                if (xDpi < 120)
                {
                    this.windowDpi = "Smaller";
                }
                else if (xDpi < 144)
                {
                    this.windowDpi = "Medium";
                }
                else
                {
                    this.windowDpi = "Larger";
                }
            }

            return this.windowDpi;
        }
    }
}
