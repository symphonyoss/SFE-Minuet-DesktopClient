using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Paragon.Plugins;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime
{
    public class WorkingSetMonitor : IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly int _highThreshold;
        private readonly int _mediumThreshold;
        private bool _disposed;
        private bool _initialEmptyComplete;
        private long _lastEmptyTimestamp;
        private Timer _timer;

        public WorkingSetMonitor(int mediumThreshold, int highThreshold)
        {
            _mediumThreshold = mediumThreshold;
            _highThreshold = highThreshold;
            _timer = new Timer(TimerCallback, null, 20000, Timeout.Infinite);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Thread.MemoryBarrier();
            _timer.Dispose();
            _timer = null;
        }

        private void TimerCallback(object state)
        {
            try
            {
                // Get process handles.
                var process = Process.GetCurrentProcess();
                var pid = process.Id;
                var handle1 = (long)process.Handle;

                if (!_initialEmptyComplete)
                {
                    // The first time through, empty the working set and return.
                    _initialEmptyComplete = true;
                    NativeMethods.EmptyWorkingSet(handle1);
                    return;
                }

                // Get memory usage.
                var safeHandle = SafeProcessHandle.OpenProcess(pid, ProcessAccessFlags.QueryInformation);
                var handle2 = safeHandle.DangerousGetHandle();
                var counters = new PROCESS_MEMORY_COUNTERS();
                NativeMethods.GetProcessMemoryInfo(handle2, out counters, (uint)Marshal.SizeOf(counters));

                var workingSetSize = counters.WorkingSetSize / 1024 / 1024;
                if (workingSetSize < _mediumThreshold)
                {
                    // We're below the medium threshold, nothing to do.
                    return;
                }

                if (workingSetSize < _highThreshold)
                {
                    // We're above the medium threshold and below the high threshold. Get the 
                    // time since the last empty and bail if it was less than 10 minutes ago.
                    var timeSinceLastEmpty = TimeSpan.FromTicks(
                        Stopwatch.GetTimestamp() - _lastEmptyTimestamp);

                    if (timeSinceLastEmpty < TimeSpan.FromMinutes(10))
                    {
                        return;
                    }
                }

                // Empty the working set.
                NativeMethods.EmptyWorkingSet(handle1);
                _lastEmptyTimestamp = Stopwatch.GetTimestamp();

                // Get updated memory usage.
                NativeMethods.GetProcessMemoryInfo(handle2, out counters, (uint)Marshal.SizeOf(counters));

                // Record the operation in the log.
                var newWorkingSetSize = counters.WorkingSetSize / 1024 / 1024;
                var highUsageDetected = workingSetSize > _highThreshold;
                    
                Logger.Info(
                    "{0} memory threshold ({1}MB) exceeded - working set reduced from: {2} to: {3}", 
                    highUsageDetected ? "High" : "Medium",
                    highUsageDetected ? _highThreshold : _mediumThreshold,
                    workingSetSize, 
                    newWorkingSetSize);
            }
            catch (Exception e)
            {
                Logger.Error("Error monitoring working set size", e);
            }
            finally
            {
                Thread.MemoryBarrier();
                if (!_disposed)
                {
                    _timer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(-1));
                }
            }
        }
    }
}