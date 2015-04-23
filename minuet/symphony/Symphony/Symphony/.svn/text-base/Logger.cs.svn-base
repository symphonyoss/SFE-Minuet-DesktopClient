using System.Diagnostics;

namespace Symphony
{
    public static class Logger
    {
        public static void Debug(string format, params object[] args)
        {
            Write(TraceEventType.Verbose, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Write(TraceEventType.Error, format, args);
        }

        public static void Info(string format, params object[] args)
        {
            Write(TraceEventType.Information, format, args);
        }

        public static void Warn(string format, params object[] args)
        {
            Write(TraceEventType.Warning, format, args);
        }

        private static void Write(TraceEventType eventType, string format, params object[] args)
        {
            //var level = (TraceEventType)ParagonRuntimeTraceSources.Default.Switch.Level;
            //if (!level.HasFlag(eventType))
            //{
            //    return;
            //}

            //var message = format;
            //if (args.Length > 0)
            //{
            //    try
            //    {
            //        message = string.Format(format, args);
            //    }
            //    catch (Exception e)
            //    {
            //        message = "Error formatting log message: " + e;
            //    }
            //}

            //ParagonRuntimeTraceSources.Default.TraceEvent(eventType, 0, message);
        }
    }
}
