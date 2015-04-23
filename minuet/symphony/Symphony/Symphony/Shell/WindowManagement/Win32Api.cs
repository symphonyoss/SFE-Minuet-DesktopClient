using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Symphony.Shell.WindowManagement
{
    public static class Win32Api
    {
        private static readonly Encoding Encoding = new UTF8Encoding();
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(WindowPlacement));

        public static string GetWindowPlacement(IntPtr winHandle)
        {
            WindowPlacement placement;
            NativeMethods.GetWindowPlacement(winHandle, out placement);

            return SerializeWindowPlacement(placement);
        }

        public static void SetWindowPlacement(IntPtr winHandle, string placementXml)
        {
            if (string.IsNullOrEmpty(placementXml))
            {
                return;
            }

            try
            {
                var placement = DeSerializeWindowPlacement(placementXml);
                NativeMethods.SetWindowPlacement(winHandle, ref placement);
            }
            catch (InvalidOperationException)
            {

            }
        }

        public static WindowPlacement DeSerializeWindowPlacement(string placementXml)
        {
            byte[] xmlBytes = Encoding.GetBytes(placementXml);

            WindowPlacement placement;
            using (var memStream = new MemoryStream(xmlBytes))
            {
                placement = (WindowPlacement)Serializer.Deserialize(memStream);
            }

            placement.length = Marshal.SizeOf(typeof(WindowPlacement));
            placement.flags = 0;

            return placement;
        }

        public static string SerializeWindowPlacement(WindowPlacement placement)
        {
            using (var memStream = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(memStream, Encoding.UTF8))
                {
                    Serializer.Serialize(writer, placement);

                    var xmlBytes = memStream.ToArray();
                    return Encoding.GetString(xmlBytes);
                }
            }
        }
    }
}
