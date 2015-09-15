using System;
using System.Diagnostics;
using Paragon.Plugins;
using Paragon.Runtime.Annotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Paragon.Runtime.Win32
{
    public class SystemCpuInfo
    {
        private static readonly ILogger _logger = ParagonLogManager.GetLogger();
        private static readonly string ArchitectureVal;
        private static readonly string CpuNameVal;
        private static readonly int ProcessorCountVal;
        private static readonly string PlatformVal;
        private static PerformanceCounter CpuCounter;

        static SystemCpuInfo()
        {
            // Store backing values for instance fields to support serialization.
            ArchitectureVal = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine);
            CpuNameVal = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            PlatformVal = Environment.GetEnvironmentVariable("BuildPlatform");
            ProcessorCountVal = Environment.ProcessorCount;
            InitializePerformanceCounters();
        }

        public SystemCpuInfo()
        {
            CpuUsage = (CpuCounter != null) ? (Math.Round(CpuCounter.NextValue())) : (0);
        }

        public double CpuUsage { get; set; }

        [UsedImplicitly]
        public string Architecture
        {
            get { return ArchitectureVal; }
        }

        [UsedImplicitly]
        public string CpuName
        {
            get { return CpuNameVal; }
        }

        [UsedImplicitly]
        public string Platform
        {
            get { return PlatformVal; }
        }

        [UsedImplicitly]
        public int ProcessorCount
        {
            get { return ProcessorCountVal; }
        }

        private static void InitializePerformanceCounters()
        {
            try
            {
                CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to initialize performance counters: {0}", ex.ToString()));
            }
        }
    }
}