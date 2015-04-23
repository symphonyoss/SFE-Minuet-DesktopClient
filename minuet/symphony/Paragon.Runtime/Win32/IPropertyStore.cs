using System;
using System.Runtime.InteropServices;

namespace Paragon.Runtime.Win32
{
    [ComImport,
     Guid(WindowPropertyStore.PropertyStoreIid),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        void GetCount(out UInt32 cProps);

        void GetAt(UInt32 iProp, [MarshalAs(UnmanagedType.Struct)] out PropertyKey pkey);

        void GetValue([In, MarshalAs(UnmanagedType.Struct)] ref PropertyKey pkey,
            [Out, MarshalAs(UnmanagedType.Struct)] out PropVariant pv);

        void SetValue([In, MarshalAs(UnmanagedType.Struct)] ref PropertyKey pkey,
            [In, MarshalAs(UnmanagedType.Struct)] ref PropVariant pv);

        void Commit();
    }
}