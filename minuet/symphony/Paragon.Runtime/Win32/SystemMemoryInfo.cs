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
using System.Security.Permissions;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Paragon.Runtime.Win32
{
    public class SystemMemoryInfo
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public SystemMemoryInfo()
        {
            var pi = new PERFORMANCE_INFORMATION();
            if (NativeMethods.GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                var pageSize = (long) pi.PageSize;
                var f = new Func<UIntPtr, long>(p => (long) p*pageSize);

                CommitTotalPages = f(pi.CommitTotal);
                CommitLimitPages = f(pi.CommitLimit);
                CommitPeakPages = f(pi.CommitPeak);
                PhysicalTotalBytes = f(pi.PhysicalTotal);
                PhysicalAvailableBytes = f(pi.PhysicalAvailable);
                PhysicalUsedBytes = PhysicalTotalBytes - PhysicalAvailableBytes;
                SystemCacheBytes = f(pi.SystemCache);
                KernelTotalBytes = f(pi.KernelTotal);
                KernelPagedBytes = f(pi.KernelPaged);
                KernelNonPagedBytes = f(pi.KernelNonpaged);
                PageSizeBytes = pageSize;
                MemoryUsage = Math.Round(((double) PhysicalUsedBytes/PhysicalTotalBytes)*100);
                HandlesCount = (int) pi.HandleCount;
                ProcessCount = (int) pi.ProcessCount;
                ThreadCount = (int) pi.ThreadCount;
            }
        }

        public long CommitTotalPages { get; set; }
        public long CommitLimitPages { get; set; }
        public long CommitPeakPages { get; set; }
        public long PhysicalTotalBytes { get; set; }
        public long PhysicalAvailableBytes { get; set; }
        public long PhysicalUsedBytes { get; set; }
        public long SystemCacheBytes { get; set; }
        public long KernelTotalBytes { get; set; }
        public long KernelPagedBytes { get; set; }
        public long KernelNonPagedBytes { get; set; }
        public long PageSizeBytes { get; set; }
        public double MemoryUsage { get; set; }
        public int HandlesCount { get; set; }
        public int ProcessCount { get; set; }
        public int ThreadCount { get; set; }
    }
}