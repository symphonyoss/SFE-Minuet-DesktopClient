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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Paragon.Plugins;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Paragon.Runtime.Win32
{
    public sealed class ProcessPerfInfo : IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private bool _disposed;
        private DateTime _lastCpuCheck;
        private TimeSpan _previousTotalProcTime;
        private PerformanceCounter _committedBytesCounter;
        private bool _countersInitialized;

        public ProcessPerfInfo(int pid)
        {
            Pid = pid;
        }

        public string CpuTime { get; set; }
        public long ManagedBytes { get; set; }
        public long PageFaultCount { get; set; }
        public long PageFileUsage { get; set; }
        public long PeakPageFileUsage { get; set; }
        public long PeakWorkingSet { get; set; }
        public string PercentCpu { get; set; }
        public int Pid { get; private set; }
        public long PrivateBytes { get; set; }
        public long WorkingSet { get; set; }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Dispose()
        {
            if (_committedBytesCounter != null)
            {
                _committedBytesCounter.Dispose();
                _committedBytesCounter = null;
            }

            _disposed = true;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void Refresh(DateTime now)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                using (var processHandle = SafeProcessHandle.OpenProcess(Pid, ProcessAccessFlags.QueryInformation))
                {
                    unchecked
                    {
                        var counters = new PROCESS_MEMORY_COUNTERS();

                        NativeMethods.GetProcessMemoryInfo(processHandle.DangerousGetHandle(),
                            out counters, (uint)Marshal.SizeOf(counters));

                        WorkingSet = counters.WorkingSetSize;
                        PageFaultCount = counters.PageFaultCount;
                        PageFileUsage = counters.PagefileUsage;
                        PeakPageFileUsage = counters.PeakPagefileUsage;
                        PeakWorkingSet = counters.PeakWorkingSetSize;
                        PrivateBytes = counters.PrivateUsage;

                        long createTime = 0;
                        long exitTime = 0;
                        long kernelTime = 0;
                        long userTime = 0;

                        NativeMethods.GetProcessTimes(processHandle.DangerousGetHandle(),
                            ref createTime, ref exitTime, ref kernelTime, ref userTime);

                        var cpuTime = new TimeSpan(kernelTime + userTime);
                        CpuTime = string.Format(@"{0:00}:{1:00}:{2:00}", cpuTime.Hours, cpuTime.Minutes, cpuTime.Seconds);

                        var latestProcTime = cpuTime - _previousTotalProcTime;
                        var latestWallTime = (now - _lastCpuCheck).TotalMilliseconds;
                        _previousTotalProcTime = cpuTime;
                        _lastCpuCheck = now;

                        if (latestWallTime > 0)
                        {
                            var totalCpu = 100 * latestProcTime.TotalMilliseconds / latestWallTime;
                            var adjustedCpu = (int)Math.Round(totalCpu / Environment.ProcessorCount);
                            PercentCpu = adjustedCpu.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            PercentCpu = "0";
                        }
                    }
                }

                if (!_countersInitialized)
                {
                    InitializePerformanceCounters();
                }

                ManagedBytes = (_committedBytesCounter != null) ? ((long)_committedBytesCounter.NextValue()) : (0);
            }
            catch (Exception e)
            {
                Logger.Error("Error refreshing process performance info", e);
            }
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                string instanceName = null;
                var category = new PerformanceCounterCategory("Process");
                var names = category.GetInstanceNames();

                foreach (var name in names)
                {
                    using (var counter = new PerformanceCounter("Process", "ID Process", name, true))
                    {
                        if (Pid == (int)counter.RawValue)
                        {
                            instanceName = name;
                            break;
                        }
                    }
                }

                if (instanceName != null)
                {
                    _committedBytesCounter = new PerformanceCounter(
                        ".NET CLR Memory", "# Total committed Bytes", instanceName);

                    _countersInitialized = true;
                }
                else
                {
                    Logger.Warn("Unable to determine process instance name for committed CLR bytes performanc counter");
                }
            }
            catch (Exception e)
            {
                Logger.Warn("Error initializing committed CLR bytes counter: " + e.Message);
            }
        }
    }
}