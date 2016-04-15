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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Paragon.Runtime;
using Xilium.CefGlue;
using System.Threading;
using System.Runtime.InteropServices;

namespace Paragon.Renderer
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            InitLogging(args);
            var logger = ParagonLogManager.GetLogger();
            var procId = Process.GetCurrentProcess().Id;
            logger.Info("Render process {0} starting", procId);

            try
            {
                if (!ExitWhenParentProcessExits())
                {
                    logger.Warn("Unable to monitor parent process for exit");
                }

                CefRuntime.Load();
                CefRuntime.ExecuteProcess(new CefMainArgs(args), new CefRenderApplication(), IntPtr.Zero);
            }
            catch (Exception ex)
            {
                logger.Error("Fatal error in render process {0} : {1}, StackTrace = {2}", procId, ex.Message, ex.StackTrace);
                return 1;
            }

            logger.Info("Render process {0} stopping.", procId);
            return 0;
        }

        private static void InitLogging(IEnumerable<string> args)
        {
            var logFileArg = args.FirstOrDefault(a => a.StartsWith("--log-file="));
            if (string.IsNullOrEmpty(logFileArg))
            {
                return;
            }

            var startIdx = logFileArg.IndexOf("=", StringComparison.OrdinalIgnoreCase);
            if (startIdx == -1)
            {
                return;
            }

            startIdx++;
            var cefLogPath = logFileArg.Substring(startIdx, logFileArg.Length - startIdx);
            var logDir = Path.GetDirectoryName(cefLogPath);
            if (string.IsNullOrEmpty(logDir))
            {
                return;
            }

            ParagonLogManager.ConfigureLogging(logDir, LogContext.Renderer, 5);
        }

        private static bool ExitWhenParentProcessExits()
        {
            int parentProcessPid;

            try
            {
                var currentProc = Process.GetCurrentProcess();
                var processInfo = new PROCESS_BASIC_INFORMATION();
                uint bytesWritten;

                NtQueryInformationProcess(currentProc.Handle, 0, ref processInfo,
                    (uint)Marshal.SizeOf(processInfo), out bytesWritten);

                parentProcessPid = processInfo.ParentPid;
            }
            catch (Exception)
            {
                return false;
            }

            var thread = new Thread(() =>
            {
                var parentProc = Process.GetProcessById(parentProcessPid);
                parentProc.WaitForExit(int.MaxValue);
                Process.GetCurrentProcess().Kill();
            }) { IsBackground = true };

            thread.Start();
            return true;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr hProcess, int processInformationClass,
            ref PROCESS_BASIC_INFORMATION processBasicInformation, uint processInformationLength, out uint returnLength);


        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public int ExitStatus;
            public int PebBaseAddress;
            public int AffinityMask;
            public int BasePriority;
            public int Pid;
            public int ParentPid;
        }

    }
}