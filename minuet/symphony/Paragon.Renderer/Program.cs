using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Paragon.Runtime;
using Xilium.CefGlue;

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
                CefRuntime.Load();

                using (new WorkingSetMonitor(120, 180))
                {
                    CefRuntime.ExecuteProcess(new CefMainArgs(args), new CefRenderApplication(), IntPtr.Zero);
                }
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
    }
}