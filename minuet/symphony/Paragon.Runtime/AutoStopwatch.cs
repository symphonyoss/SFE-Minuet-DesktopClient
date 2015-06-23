using System;
using System.Diagnostics;
using Paragon.Plugins;

namespace Paragon.Runtime
{
    /// <summary>
    /// Stopwatch utility used to time operations and write log messages in cases
    /// where the elapsed time exceeds specified limits.
    /// </summary>
    public sealed class AutoStopwatch : IDisposable
    {
        private const int DefaultThreshold = 250;
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly string _message;
        private readonly Stopwatch _stopwatch;
        private readonly int _threshold;
        private bool _disposed;

        private AutoStopwatch(string message, int threshold)
        {
            _message = message;
            _threshold = threshold;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _stopwatch.Stop();

            if (_threshold == -1)
            {
                Logger.Info("{0} took {1}ms", _message, _stopwatch.ElapsedMilliseconds);
            }
            else if (_stopwatch.ElapsedMilliseconds > _threshold)
            {
                // The elaspsed time is greater than the specified threshold,
                // write a log message at WARN level.
                Logger.Warn("{0} completed in {1}ms, exceeding threshold of {2}ms",
                    _message, _stopwatch.ElapsedMilliseconds, _threshold);
            }
        }

        /// <summary>
        /// Initialize a new instance and begin timing.
        /// </summary>
        /// <param name="message">Text describing the operation being timed to use when writing to the log</param>
        /// <param name="threshold">Time threshold in milliseconds. If the operation being timed exceeds this value, a log message will be written to record it.</param>
        /// <returns>A new AutoStopwatch instance</returns>
        public static AutoStopwatch WarnIfOverThreshold(string message, int threshold = DefaultThreshold)
        {
            return new AutoStopwatch(message, threshold);
        }

        /// <summary>
        /// Initialize a new instance and begin timing.
        /// </summary>
        /// <param name="message">Text describing the operation being timed to use when writing to the log</param>
        /// <returns>A new AutoStopwatch instance</returns>
        public static AutoStopwatch TimeIt(string message)
        {
            return new AutoStopwatch(message, -1);
        }
    }
}