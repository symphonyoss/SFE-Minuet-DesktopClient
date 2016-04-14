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