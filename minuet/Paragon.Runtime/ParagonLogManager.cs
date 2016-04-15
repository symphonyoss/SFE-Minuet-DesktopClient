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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Logging;
using Paragon.Plugins;
using Timer = System.Threading.Timer;

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
        private static string _pid;
        private static bool _loggingConfigured;

        public static string LogDirectory
        {
            get { return _logDirectory; }    
        }

        public static string CurrentAppLogFile
        {
            get { return _appTraceListener.FullLogFileName; }
        }

        public static string CurrentParagonLogFile
        {
            get { return _paragonTraceListener.FullLogFileName; }
        }

        public static void AddApplicationTraceListener(string appId)
        {
            // Max file size for log files is 10MB (the default is 5MB if not explicitly set).
            const int maxFileSize = 10 * 1024 * 1024;

            _appTraceListener = new FileLogTraceListener
            {
                AutoFlush = true,
                Location = LogFileLocation.Custom,
                CustomLocation = _logDirectory,
                LogFileCreationSchedule = LogFileCreationScheduleOption.Daily,
                BaseFileName = "application-" + appId,
                DiskSpaceExhaustedBehavior = DiskSpaceExhaustedOption.DiscardMessages,
                MaxFileSize = maxFileSize
            };

            // Write app trace source messages to an application log file.
            ParagonTraceSources.App.Listeners.Add(_appTraceListener);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void ConfigureLogging(string logsDir, LogContext context, int maxRolledFiles)
        {
            if (!_loggingConfigured)
            {
                // Init trace sources specific to the browser or render context
                // that is currently running.
                _logDirectory = Environment.ExpandEnvironmentVariables(logsDir);

                _pid = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture).PadLeft(5);

                // Max file size for log files is 10MB (the default is 5MB if not explicitly set).
                const int maxFileSize = 10*1024*1024;

                switch (context)
                {
                    case LogContext.Browser:
                        CleanupCefLog();

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

                        CleanupLogFiles(_logDirectory, maxRolledFiles, "paragon", "application-*");
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

                _loggingConfigured = true;
            }
        }

        public static string[] GetAppLogFiles()
        {
            return Directory.GetFiles(_logDirectory, "application-*.log").ToArray();
        }

        public static ILogger GetAppLogger(string appId)
        {
            return Loggers.GetOrAdd(appId, _ => new Logger(appId, AppLogWriter));
        }

        public static ILogger GetLogger()
        {
            var stackFrame = new StackFrame(1, false);
            var method = stackFrame.GetMethod();
            var declaringType = method.DeclaringType;
            var typeName = declaringType != null ? declaringType.FullName : "Unknown type";
            return Loggers.GetOrAdd(typeName, _ => new Logger(typeName, LogWriter));
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

        private static void CleanupCefLog()
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
            private readonly string _loggerName;
            private readonly AsyncLogWriter _writer;

            public Logger(string loggerName, AsyncLogWriter writer)
            {
                _loggerName = loggerName;
                _writer = writer;
                Level = ParagonTraceSources.Default.Switch.Level;
            }
       
            #region ILogger Members

            public void Debug(string message, string caller = null)
            {
                Write(SourceLevels.Verbose, "DEBUG", message);
            }

            public void Debug(FormatMessageCallback formatter, string caller = null)
            {
                throw new NotImplementedException();
            }

            public void Debug(string format, params object[] args)
            {
                format = EscapeCurlyBraces(format, args);
                Write(SourceLevels.Verbose, "DEBUG", () => string.Format(format, args));
            }

            public void Info(string message, string caller = null)
            {
                Write(SourceLevels.Information, "INFO ", message);
            }

            public void Info(FormatMessageCallback formatter, string caller = null)
            {
                throw new NotImplementedException();
            }

            public void Info(string format, params object[] args)
            {
                format = EscapeCurlyBraces(format, args);
                Write(SourceLevels.Information, "INFO ", () => string.Format(format, args));
            }

            public void Warn(string message, string caller = null)
            {
                Write(SourceLevels.Warning, "WARN ", message);
            }

            public void Warn(FormatMessageCallback formatter, string caller = null)
            {
                throw new NotImplementedException();
            }

            public void Warn(string format, params object[] args)
            {
                format = EscapeCurlyBraces(format, args);
                Write(SourceLevels.Warning, "WARN ", () => string.Format(format, args));
            }

            public void Error(string message, string caller = null)
            {
                Write(SourceLevels.Error, "ERROR", message);
            }

            public void Error(string message, Exception exception, string caller = null)
            {
                Write(SourceLevels.Error, "ERROR", message, exception);
            }

            public void Error(FormatMessageCallback formatter, string caller = null)
            {
                throw new NotImplementedException();
            }

            public void Error(FormatMessageCallback formatter, Exception exception, string caller = null)
            {
                throw new NotImplementedException();
            }

            public void Error(string format, params object[] args)
            {
                format = EscapeCurlyBraces(format, args);
                Write(SourceLevels.Error, "ERROR", () => string.Format(format, args));
            }

            public void Fatal(string message, string caller = null)
            {
                Write(SourceLevels.Critical, "FATAL", message);
            }

            public void Fatal(FormatMessageCallback formatter, string caller = null)
            {
                throw new NotImplementedException();
            }

            public void Fatal(string format, params object[] args)
            {
                format = EscapeCurlyBraces(format, args);
                Write(SourceLevels.Critical, "FATAL", () => string.Format(format, args));
            }

            public SourceLevels Level { get; set; }

            #endregion

            /// <summary>
            /// If we try to log JSON we would get an error message due to the curly braces being
            /// passed to string.Format. We therefore escape curly braces that we know not to be
            /// placeholders within the format string.
            /// </summary>
            /// <param name="formatString"></param>
            /// <param name="args"></param>
            /// <returns></returns>
            private string EscapeCurlyBraces(string formatString, params object[] args)
            {
                if (args.Length == 0)
                {
                    formatString = formatString.Replace("{", "{{").Replace("}", "}}");
                }
                // else maybe count args and don't escape braces where {n < args.Length}
                return formatString;
            }

            private void Write(SourceLevels msgLevel, string msgLevelPrefix, string message, Exception exception = null)
            {
                Write(msgLevel, msgLevelPrefix, () => message, exception);
            }

            private void Write(SourceLevels msgLevel, string msgLevelPrefix, Func<string> getMsg, Exception exception = null)
            {
                if ((Level & msgLevel) != msgLevel)
                {
                    return;
                }

                var ts = DateTime.Now;
                var threadId = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture).PadLeft(2);

                var getMessage = new Func<string>(() =>
                {
                    string text;

                    try
                    {
                        text = getMsg();

                        if (exception != null)
                        {
                            text = string.Concat(text, Environment.NewLine, "EXCEPTION: ", exception.ToString());
                        }

                    }
                    catch (Exception e)
                    {
                        text = "Error formatting log message: " + e.Message;
                    }

                    return string.Format(
                        "{0} {1} {2} {3} {4} {5}",
                        ts.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        _pid,
                        threadId,
                        msgLevelPrefix,
                        _loggerName,
                        text);
                });

                _writer.Write(getMessage);
            }
        }
    }
}