using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Explicit)]
    internal struct PropVariant
    {
        [FieldOffset(0)]
        private ushort vt;

        [FieldOffset(8)]
        private IntPtr pointerValue;

        [FieldOffset(8)]
        private byte byteValue;

        [FieldOffset(8)]
        private long longValue;

        [FieldOffset(8)]
        private short boolValue;

        [MarshalAs(UnmanagedType.Struct)]
        [FieldOffset(8)]
        private CALPWSTR calpwstr;

        public VarEnum VarType
        {
            get { return (VarEnum) vt; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void SetValue(String val)
        {
            Clear();
            vt = (ushort) VarEnum.VT_LPWSTR;
            pointerValue = Marshal.StringToCoTaskMemUni(val);
        }

        public void SetValue(bool val)
        {
            Clear();
            vt = (ushort) VarEnum.VT_BOOL;
            boolValue = val ? (short) -1 : (short) 0;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public string GetStringValue()
        {
            return Marshal.PtrToStringUni(pointerValue);
        }

        public bool GetBoolValue()
        {
            return boolValue == -1;
        }

        public void Clear()
        {
            NativeMethods.PropVariantClear(ref this);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct CALPWSTR
    {
        [FieldOffset(0)]
        internal uint cElems;

        [FieldOffset(4)]
        internal IntPtr pElems;
    }
}