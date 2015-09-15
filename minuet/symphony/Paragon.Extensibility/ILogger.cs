using System;
using System.Diagnostics;

namespace Paragon.Plugins
{
    public delegate void FormatMessageCallback(MessageFormatter formatter);

    public delegate string MessageFormatter(string format, params object[] args);

    public interface ILogger
    {
        void Debug(string message, string caller = null);
        void Debug(FormatMessageCallback formatter, string caller = null);
        void Debug(string format, params object[] args);
        void Info(string message, string caller = null);
        void Info(FormatMessageCallback formatter, string caller = null);
        void Info(string format, params object[] args);
        void Warn(string message, string caller = null);
        void Warn(FormatMessageCallback formatter, string caller = null);
        void Warn(string format, params object[] args);
        void Error(string message, string caller = null);
        void Error(string message, Exception exception, string caller = null);
        void Error(FormatMessageCallback formatter, string caller = null);
        void Error(FormatMessageCallback formatter, Exception exception, string caller = null);
        void Error(string format, params object[] args);
        void Fatal(string message, string caller = null);
        void Fatal(FormatMessageCallback formatter, string caller = null);
        void Fatal(string format, params object[] args);
        SourceLevels Level { get; set; }
    }
}