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

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct PROCESSENTRY32
    {
        private const int MAX_PATH = 260;
        public UInt32 dwSize;
        public UInt32 cntUsage;
        public UInt32 th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public UInt32 th32ModuleID;
        public UInt32 cntThreads;
        public UInt32 th32ParentProcessID;
        public Int32 pcPriClassBase;
        public UInt32 dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string szExeFile;
    }
}