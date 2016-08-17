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

using Paragon.Plugins;
using Paragon.Runtime.Desktop;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Paragon.Runtime.Win32
{
    public class MemoryDump
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private static readonly object Lock = new object();

        public static void CreateMemoryDump(int processId, string fileName, ProcessDumpType processDumpType)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "Memory dump file name not provided.");

            lock (Lock)
            {
                int result = NativeMethods.CreateDump(processId, fileName, (int) processDumpType, 0, IntPtr.Zero);

                if (result != 1)
                {
                    string msg = "Failed to create dump " + fileName;
                    throw new Win32Exception(Marshal.GetLastWin32Error(), msg);
                }

                Logger.Info(string.Format("Dump {0} created successfully ", fileName));

                var success = ConvertFileToBase64Format(fileName);

                if (!success)
                {
                    string msg = "Failed to convert dump file to base 64 format";
                    throw new Exception(msg);
                }
            }
        }

        public static void CreateMemoryDump(AppInfo appInfo, ProcessDumpType processDumpType)
        {
            if (appInfo == null)
                throw new ArgumentNullException("appInfo", "Application information not provided.");

            var browserFileName = string.Format("browser-{0}.dmp.log", appInfo.AppId);
            var browserFilePath = Path.Combine(ParagonLogManager.LogDirectory, browserFileName);
            CreateMemoryDump(appInfo.BrowserInfo.Pid, browserFilePath, processDumpType);

            var rendererFileName = string.Format("renderer-{0}.dmp.log", appInfo.AppId);
            var rendererFilePath = Path.Combine(ParagonLogManager.LogDirectory, rendererFileName);
            CreateMemoryDump(appInfo.RenderInfo.Pid, rendererFilePath, processDumpType);
        }

        private static bool ConvertFileToBase64Format(string filePath)
        {
            bool success = false;

            try
            {
                var byteArray = File.ReadAllBytes(filePath);
                var base64String = Convert.ToBase64String(byteArray);
                File.WriteAllText(filePath, base64String);
                success = true;
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Failed to convert file to base 64 format: {0}", ex));
            }

            return success;
        }

        public static void MiniDumpToFile(String fileToDump, Process process)
        {
            FileStream fsToDump = null;
            if (File.Exists(fileToDump))
                fsToDump = File.Open(fileToDump, FileMode.Append);
            else
                fsToDump = File.Create(fileToDump);

            try
            {
                NativeMethods.MiniDumpWriteDump(process.Handle, process.Id,
                    fsToDump.SafeFileHandle.DangerousGetHandle(), NativeMethods.MINIDUMP_TYPE.MiniDumpNormal,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                fsToDump.Close();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
