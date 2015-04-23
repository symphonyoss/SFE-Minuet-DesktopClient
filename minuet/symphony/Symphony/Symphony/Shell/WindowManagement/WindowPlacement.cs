using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Symphony.Shell.WindowManagement
{
    [Serializable]
    [XmlRoot("WINDOWPLACEMENT")]
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPlacement
    {
        public int length;
        public int flags;
        public int showCmd;
        public Point minPosition;
        public Point maxPosition;
        public Rect normalPosition;
    }
}