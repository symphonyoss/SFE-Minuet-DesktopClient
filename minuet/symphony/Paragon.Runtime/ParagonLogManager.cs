using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Logging;
using Paragon.Plugins;

namespace Paragon.Runtime
{
    public enum LogContext
    {
        Browser,
        Renderer
    }

    public static class ParagonLogManager
    {
        private static readonly ConcurrentDictionary<string, ILogger> Loggers =
            new ConcurrentDictionary<string, ILogger>();

        private static readonly AsyncLogWriter LogWriter = new AsyncLogWriter(ParagonTraceSources.Default);
        private static readonly AsyncLogWriter AppLogWriter = new AsyncLogWriter(ParagonTraceSources.App);
        private static FileLogTraceListener _paragonTraceListener;
        private static FileLogTraceListener _appTraceListener;
        private static FileLogTraceListener _rendererTraceListener;
        private static Timer _cleanupTimer;
        private static string _logDirectory;
        private static bool _stopped;

        public static string CurrentAppLogFile
        {
            get { return _appTraceListener.FullLogFileName; }
        }

        public static string CurrentParagonLogFile
        {
            get { return _paragonTraceListener.FullLogFileName; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void ConfigureLogging(string logsDir, LogContext context, int maxRolledFiles)
        {
            // Init trace sources specific to the browser or render context
            // that is currently running.
            _logDirectory = Environment.ExpandEnvironmentVariables(logsDir);

            // Max file size for log files is 10MB (the default is 5MB if not explicitly set).
            const int maxFileSize = 10 * 1024 * 1024;

            switch (context)
            {
                case LogContext.Browser:
                    CleanupCefLog(_logDirectory);

                    // Write default trace source messages to a paragon log file.
                    _paragonTraceListener = new FileLogTraceListener
                    {
                        AutoFlush = true,
                        Location = LogFileLocation.Custom,
                        CustomLocation = _logDirectory,
                        LogFileCreationSchedule = LogFileCreationScheduleOption.Daily,
                        BaseFileName = "paragon",
                        DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.DiscardMessages,
                        MaxFileSize = maxFileSize
                    };

                    ParagonTraceSources.Default.Listeners.Add(_paragonTraceListener);

                    _appTraceListener = new FileLogTraceListener
                    {
                        AutoFlush = true,
                        Location = LogFileLocation.Custom,
                        CustomLocation = _logDirectory,
                        LogFileCreationSchedule = LogFileCreationScheduleOption.Daily,
                        BaseFileName = "application",
                        DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.DiscardMessages,
                        MaxFileSize = 10 * 1024 * 1024
                    };

                    // Write app trace source messages to an application log file.
                    ParagonTraceSources.App.Listeners.Add(_appTraceListener);

                    CleanupLogFiles(_logDirectory, maxRolledFiles, "paragon", "application");
                    break;

                case LogContext.Renderer:
                    _rendererTraceListener = new FileLogTraceListener
                    {
                        AutoFlush = true,
                        Location = LogFileLocation.Custom,
                        CustomLocation = _logDirectory,
                        LogFileCreationSchedule = LogFileCreationScheduleOption.Daily,
                        BaseFileName = "renderer",
                        DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.DiscardMessages,
                        MaxFileSize = maxFileSize
                    };

                    // Write default trace source messages to a renderer log file.
                    ParagonTraceSources.Default.Listeners.Add(_rendererTraceListener);
                    CleanupLogFiles(_logDirectory, maxRolledFiles, "renderer");
                    break;
            }
        }

        public static string[] GetAppLogFiles()
        {
            return Directory.GetFiles(_logDirectory, "application*.log").ToArray();
        }

        public static ILogger GetAppLogger(string appId)
        {
            return Loggers.GetOrAdd(appId, _ => new Logger(__ => appId, AppLogWriter));
        }

        public static ILogger GetLogger()
        {
            var stackFrame = new StackFrame(1, false);
            var method = stackFrame.GetMethod();
            var declaringType = method.DeclaringType;
            var typeName = declaringType != null ? declaringType.FullName : "Unknown type";

            return Loggers.GetOrAdd(typeName, _ => new Logger(caller =>
                string.Concat(typeName, ".", caller), LogWriter));
        }

        public static void Shutdown()
        {
            if (_stopped)
            {
                return;
            }

            _stopped = true;
            AppLogWriter.Dispose();
            LogWriter.Dispose();

            if (_paragonTraceListener != null)
            {
                _paragonTraceListener.Dispose();
            }

            if (_appTraceListener != null)
            {
                _appTraceListener.Dispose();
            }

            if (_rendererTraceListener != null)
            {
                _rendererTraceListener.Dispose();
            }
        }

        private static void CleanupCefLog(string logDir)
        {
            var logger = GetLogger();

            try
            {
                var cefLogPath = Path.Combine(_logDirectory, "cef.log");
                var cefLogFile = new FileInfo(cefLogPath);
                if (!cefLogFile.Exists)
                {
                    return;
                }

                // Max allowed file size of 20MB.
                const int maxFileSize = 20 * 1024 * 1024;
                if (cefLogFile.Length > maxFileSize)
                {
                    try
                    {
                        cefLogFile.Delete();
                    }
                    catch (IOException)
                    {
                        logger.Warn("Unable to delete cef.log file as the file is in use");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Error cleaningup CEF log", e);
            }
        }

        private static void CleanupLogFiles(string logDir, int maxRolledFiles, params string[] filePrefixes)
        {
            var now = DateTime.Now;
            var logger = GetLogger();
            logger.Info("Performing log file cleanup");

            try
            {
                if (!Directory.Exists(logDir))
                {
                    return;
                }

                foreach (var prefix in filePrefixes)
                {
                    var files = Directory.GetFiles(logDir, prefix + "*", SearchOption.TopDirectoryOnly).ToList();
                    if (files.Count <= maxRolledFiles + 1)
                    {
                        continue;
                    }

                    // File names are always in the format BaseName-yyyy-MM-dd.log regardless of locale.
                    // Sort (ascending sort is the default) and delete the first n files until we're within
                    // the limit specified by maxRolledFiles (+1 to account for the current file).
                    files.Sort();
                    var filesToDelete = files.Take(files.Count - maxRolledFiles - 1).ToList();
                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            logger.Info("Deleting archived log file: " + file);
                            File.Delete(file);
                        }
                        catch (Exception e)
                        {
                            logger.Error("Error deleting log file", e);
                        }
                    }
                }
            }
            catch (Exception e)
            {

                logger.Error("Error performing log file cleanup", e);
            }
            finally
            {
                // Schedule the next cleanup for midnight tonight.
                var tomorrow = now.AddDays(1);
                var durationUntilMidnight = tomorrow.Date - now;

                if (_cleanupTimer == null)
                {
                    _cleanupTimer = new Timer(o => CleanupLogFiles(logDir, maxRolledFiles, filePrefixes),
                        null, (long)durationUntilMidnight.TotalMilliseconds, -1);
                }
                else
                {
                    _cleanupTimer.Change((long)durationUntilMidnight.TotalMilliseconds, -1);
                }
            }
        }

        /// <summary>
        /// Used to offload log writes to a background thread. The Write() method takes a lambda that returns
        /// a string so that the message formatting is also performed on the background thread.
        /// </summary>
        private class AsyncLogWriter : IDisposable
        {
            private readonly BlockingCollection<Func<string>> _queue = new BlockingCollection<Func<string>>();
            private readonly CancellationTokenSource _stopSignal = new CancellationTokenSource();
            private readonly TraceSource _traceSource;
            private Task _processingTask;
            private bool _disposed;

            public AsyncLogWriter(TraceSource traceSource)
            {
                _traceSource = traceSource;
                _processingTask = Task.Factory.StartNew(ConsumeQueue);
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _stopSignal.Cancel();
                _queue.CompleteAdding();
                _processingTask.Wait();
                _queue.Dispose();
                _processingTask = null;
            }

            public void Write(Func<string> getMessage)
            {
                if (_disposed)
                {
                    return;
                }

                _queue.Add(getMessage);
            }

            [DebuggerStepThrough]
            private void ConsumeQueue()
            {
                try
                {
                    foreach (var messageFunc in _queue.GetConsumingEnumerable(_stopSignal.Token))
                    {
                        WriteMessage(messageFunc);
                    }
                }
                catch (OperationCanceledException)
                {
                    foreach (var messageFunc in _queue)
                    {
                        WriteMessage(messageFunc);
                    }
                }
            }

            private void WriteMessage(Func<string> getMessage)
            {
                string message;

                try
                {
                    // Build the log message string.
                    message = getMessage();
                }
                catch
                {
                    return;
                }

                // Iterate over all listeners attached to the TraceSource and 
                // write the message to each of them. Cast and convert to array
                // to prevent collection changed exceptions.
                var listeners = _traceSource.Listeners.Cast<TraceListener>().ToArray();
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener.WriteLine(message);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private class Logger : ILogger
        {
            private readonly Func<string, string> _loggerName;
            private readonly AsyncLogWriter _writer;

            public Logger(Func<string, string> loggerName, AsyncLogWriter writer)
            {
                _loggerName = loggerName;
                _writer = writer;
            }

            public void Debug(string message, string caller = null)
            {
                Write(TraceEventType.Verbose, "DEBUG", message, caller);
            }

            public void Debug(FormatMessageCallback formatter, string caller = null)
            {
                Write(TraceEventType.Verbose, "DEBUG", formatter, caller);
            }

            public void Error(string message, string caller = null)
            {
                Write(TraceEventType.Error, "ERROR", message, caller);
            }

            public void Error(string message, Exception exception, string caller = null)
            {
                Write(TraceEventType.Error, "ERROR", message, caller, exception);
            }

            public void Error(FormatMessageCallback formatter, string caller = null)
            {
                Write(TraceEventType.Error, "ERROR", formatter, caller);
            }

            public void Error(FormatMessageCallback formatter, Exception exception, string caller = null)
            {
                Write(TraceEventType.Error, "ERROR", formatter, caller, exception);
            }

            public void Fatal(string message, string caller = null)
            {
                Write(TraceEventType.Critical, "FATAL", message, caller);
            }

            public void Fatal(FormatMessageCallback formatter, string caller = null)
            {
                Write(TraceEventType.Critical, "FATAL", formatter, caller);
            }

            public void Info(string message, string caller = null)
            {
                Write(TraceEventType.Information, "INFO ", message, caller);
            }

            public void Info(FormatMessageCallback formatter, string caller = null)
            {
                Write(TraceEventType.Information, "INFO ", formatter, caller);
            }

            public void Warn(string message, string caller = null)
            {
                Write(TraceEventType.Warning, "WARN ", message, caller);
            }

            public void Warn(FormatMessageCallback formatter, string caller = null)
            {
                Write(TraceEventType.Warning, "WARN ", formatter, caller);
            }

            private void Write(TraceEventType eventType, string level, string message, string caller, Exception exception = null)
            {
                Write(eventType, level, fmt => fmt(message), caller, exception);
            }

            private void Write(TraceEventType eventType, string level, FormatMessageCallback formatter, string caller, Exception exception = null)
            {
                var levelFlag = (TraceEventType)ParagonTraceSources.Default.Switch.Level;
                if ((levelFlag & eventType) != eventType)
                {
                    return;
                }

                var text = new FormatMessageCallbackFormattedMessage(formatter).ToString();
                if (exception != null)
                {
                    text = string.Concat(text, Environment.NewLine, "EXCEPTION: ", exception.ToString());
                }

                if (string.IsNullOrEmpty(caller))
                {
                    caller = "Delegate";
                }

                var ts = DateTime.Now;
                var threadId = Thread.CurrentThread.ManagedThreadId;
                var getMessage = new Func<string>(() => string.Format("[{0}] {1} {2} {3} {4}",
                    threadId, ts.ToString("yyyy-MM-dd HH:mm:ss.fff"), level, _loggerName(caller), text));

                _writer.Write(getMessage);
            }

            private class FormatMessageCallbackFormattedMessage
            {
                private readonly FormatMessageCallback _formatMessageCallback;
                private volatile string _cachedMessage;

                public FormatMessageCallbackFormattedMessage(FormatMessageCallback formatMessageCallback)
                {
                    _formatMessageCallback = formatMessageCallback;
                }

                public override string ToString()
                {
                    if (_cachedMessage == null && _formatMessageCallback != null)
                    {
                        _formatMessageCallback(FormatMessage);
                    }

                    return _cachedMessage ?? "Empty log message";
                }

                private string FormatMessage(string format, params object[] args)
                {
                    if (format == null)
                    {
                        return null;
                    }

                    try
                    {
                        _cachedMessage = string.Format(format, args);
                    }
                    catch
                    {
                        _cachedMessage = format;
                    }

                    return _cachedMessage;
                }
            }
        }
    }
}