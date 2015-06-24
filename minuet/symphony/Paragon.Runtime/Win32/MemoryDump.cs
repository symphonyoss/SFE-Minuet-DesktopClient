using Paragon.Plugins;
using Paragon.Runtime.Desktop;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

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
            }
        }

        public static void CreateMemoryDump(AppInfo appInfo, ProcessDumpType processDumpType)
        {
            if (appInfo == null)
                throw new ArgumentNullException("appInfo", "Application information not provided.");

            var browserFileName = Path.Combine(ParagonLogManager.LogDirectory, "Browser" + processDumpType.ToString() + ".dmp");
            CreateMemoryDump(appInfo.BrowserInfo.Pid, browserFileName, processDumpType);

            var rendererFileName = Path.Combine(ParagonLogManager.LogDirectory, "Renderer" + processDumpType.ToString() + ".dmp");
            CreateMemoryDump(appInfo.RenderInfo.Pid, rendererFileName, processDumpType);
        }
    }
}
