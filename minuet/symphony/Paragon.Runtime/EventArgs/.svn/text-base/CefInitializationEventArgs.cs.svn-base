using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    /// <summary>
    /// CEF Initialiation event arguments.
    /// </summary>
    public class CefInitializationEventArgs : EventArgs
    {
        public CefInitializationEventArgs()
        {
            LogFile = "cef.log";
            LogSeverity = CefLogSeverity.Disable;
            RemoteDebuggingPort = -1;
        }

        /// <summary>
        /// Path to log file. Default is "cef.log" located in the Paragon installation folder.
        /// </summary>
        public string LogFile { get; set; }

        /// <summary>
        /// Log level. Default is 'Disabled'.
        /// </summary>
        public CefLogSeverity LogSeverity { get; set; }

        /// <summary>
        /// Remote debugging port. Default is -1 (disabled).
        /// </summary>
        public int RemoteDebuggingPort { get; set; }
    }
}