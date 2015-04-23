using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Paragon.Runtime.Win32
{
    internal static class StructConverter
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static T FromString<T>(string str)
        {
            var bytes = Convert.FromBase64String(str);
            return FromByteArray<T>(ref bytes);
        }

        public static string ToString<T>(T structure) where T : struct
        {
            var bytes = ToByteArray(structure);
            return Convert.ToBase64String(bytes);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private static T FromByteArray<T>(ref byte[] bytes)
        {
            var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var ptr = gch.AddrOfPinnedObject();
            var data = (T) Marshal.PtrToStructure(ptr, typeof (T));
            gch.Free();
            return data;
        }

        private static byte[] ToByteArray<T>(T structure) where T : struct
        {
            var size = Marshal.SizeOf(typeof (T));
            var bytes = new byte[size];
            var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var ptr = gch.AddrOfPinnedObject();
            Marshal.StructureToPtr(structure, ptr, false);
            gch.Free();
            return bytes;
        }
    }
}