using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using Paragon.Runtime.Annotations;

namespace Paragon.Runtime.Kernel.Plugins
{
    [JavaScriptPlugin(Name = "paragon.log")]
    public class LoggerPlugin : ParagonPlugin
    {
        private delegate void WriteToLogFormatted(string format, params object[] args);
        private ILogger _logger;

        [JavaScriptPluginMember, UsedImplicitly]
        public void SetLevel(int level)
        {
            _logger.Level = (SourceLevels) level;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public int GetLevel()
        {
            return (int)_logger.Level;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void Debug(object args)
        {
            Write(_logger.Debug, args);
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void Error(object args)
        {
            Write(_logger.Error, args);
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void Info(object args)
        {
            Write(_logger.Info, args);
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void Warn(object args)
        {
            Write(_logger.Warn, args);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _logger = ParagonLogManager.GetAppLogger(Application.Metadata.Id);
        }

        private void Write(WriteToLogFormatted formattedWriter, object args)
        {
            // We're expecting the args object to be a JObject containing the params
            // passed to one of the JavaScript log functions.
            var arguments = args as JObject;
            if (arguments == null)
            {
                return;
            }

            if (arguments.Count == 0)
            {
                // Bail, no arguments supplied or not a JObject.
                return;
            }

            // Convert all args to string.
            var argList = arguments.Values().Select(a => a.ToString()).ToList();

            // If the first arg is a string, attempt to replace formatting markers.
            if (arguments.First.Type == JTokenType.String)
            {
                var format = argList[0];
                var count = 0;
                var evaluator = new Func<Match, string>(m => m.Result(string.Concat("{", count++, "}")));
                format = Regex.Replace(format, "%s|%d|%i|%f|%o|%0", new MatchEvaluator(evaluator));

                // If count is > 0, formatting markers were replaced, meaning we have a valid format string.
                if (count > 0)
                {
                    // Format and return.
                    formattedWriter(format, argList.Skip(1).Cast<object>().ToArray());
                    return;
                }
            }

            // Concat all args with a string separator and log it.
            formattedWriter(string.Join(" ", argList.ToArray()));
        }
    }
}