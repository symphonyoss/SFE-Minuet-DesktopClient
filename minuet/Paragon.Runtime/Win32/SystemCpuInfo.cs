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