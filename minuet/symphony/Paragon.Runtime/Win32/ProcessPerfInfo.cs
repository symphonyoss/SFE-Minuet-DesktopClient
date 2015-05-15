using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Paragon.Runtime.Win32
{
    public sealed class ProcessPerfInfo : IDisposable
    {
        private bool _disposed;
        private DateTime _lastCpuCheck;
        private TimeSpan _previousTotalProcTime;
        private SafeProcessHandle _processHandle;

        public ProcessPerfInfo(int pid)
        {
            Pid = pid;
        }

        public string CpuTime { get; set; }
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
            if (_processHandle != null)
            {
                _processHandle.Dispose();
                _processHandle = null;
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

            if (_processHandle == null)
            {
                _processHandle = SafeProcessHandle.OpenProcess(Pid, ProcessAccessFlags.QueryInformation);
            }

            unchecked
            {
                var counters = new PROCESS_MEMORY_COUNTERS();

                NativeMethods.GetProcessMemoryInfo(_processHandle.DangerousGetHandle(),
                    out counters, (uint) Marshal.SizeOf(counters));

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

                NativeMethods.GetProcessTimes(_processHandle.DangerousGetHandle(),
                    ref createTime, ref exitTime, ref kernelTime, ref userTime);

                var cpuTime = new TimeSpan(kernelTime + userTime);
                // TODO : Make sure that this works
                CpuTime = string.Format(@"{0:hh\:mm\:ss}", cpuTime);

                var latestProcTime = cpuTime - _previousTotalProcTime;
                var latestWallTime = (now - _lastCpuCheck).TotalMilliseconds;
                _previousTotalProcTime = cpuTime;
                _lastCpuCheck = now;

                if (latestWallTime > 0)
                {
                    var totalCpu = 100*latestProcTime.TotalMilliseconds/latestWallTime;
                    var adjustedCpu = (int) Math.Round(totalCpu/Environment.ProcessorCount);
                    PercentCpu = adjustedCpu.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    PercentCpu = "0";
                }
            }
        }
    }
}