//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

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
    internal struct PropVariant : IDisposable
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

        public void Dispose()
        {
            Clear();
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