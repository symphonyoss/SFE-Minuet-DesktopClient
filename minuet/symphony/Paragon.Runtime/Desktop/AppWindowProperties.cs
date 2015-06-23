using System.Runtime.InteropServices;
using System.Security.Permissions;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Desktop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    internal struct AppWindowProperties
    {
        private const string Prefix = "PGN_WND:";

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 37)]
        public string WorkspaceId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 37)]
        public string AppInstanceId;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool TryParse(string value, out AppWindowProperties props)
        {
            props = default(AppWindowProperties);
            if (string.IsNullOrEmpty(value)
                || !value.StartsWith(Prefix))
            {
                return false;
            }

            props = StructConverter.FromString<AppWindowProperties>(value.Substring(8));
            return true;
        }

        public override string ToString()
        {
            return string.Concat(Prefix, StructConverter.ToString(this));
        }
    }
}