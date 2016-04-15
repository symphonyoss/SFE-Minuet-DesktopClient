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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Paragon.Plugins;

namespace Paragon.Runtime.Kernel.Applications
{
    /// <summary>
    /// Parses command line string passed to Paragon on startup. Provides convenience functions
    /// for querying content of command line and for creating application metadata.
    /// Conversely it can be used to create a command line string given a set of parameters.
    /// </summary>
    public class ParagonCommandLineParser
    {
        private const string Env = "env";
        private const string StartApp = "start-app";
        private const string StartAppId = "start-app-id";
        private const string StartAppInstId = "start-app-inst-id";
        private const string StartAppType = "start-app-type";
        private const string WDPort = "wdport";
        private const string Standalone = "standalone";
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly List<string> _args;

        public ParagonCommandLineParser(IEnumerable<string> args)
        {
            _args = args.ToList();
        }

        public bool HasFlag(string flag)
        {
            var rex = new Regex(@"^\s*((/)|(\-\-))" + flag.Replace("-", @"\-") + "[=:]*");
            return _args.Any(a => rex.Match(a).Length > 0);
        }

        public bool GetValue(string flag, out string value)
        {
            if (string.IsNullOrEmpty(flag))
            {
                throw new ArgumentNullException("flag");
            }

            var rex = new Regex(@"^\s*((/)|(\-\-))" + flag.Replace("-", @"\-") + "[=:]+");
            var arg = _args.FirstOrDefault(a => rex.Match(a).Length > 0);

            if (string.IsNullOrEmpty(arg))
            {
                value = null;
                return false;
            }

            var index = arg.IndexOf('=');
            if (index < 0)
            {
                index = arg.IndexOf(':');
            }

            value = index >= 0 && index + 1 < arg.Length ? arg.Substring(index + 1) : string.Empty;

            // If there are quotes surrounding the value, remove them
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            return true;
        }

        public ApplicationMetadata ParseApplicationMetadata()
        {
            var applicationMetadata = new ApplicationMetadata();

            // Resolve start data.
            string startApp;
            if (!GetValue(StartApp, out startApp))
            {
                Logger.Error("No --start-app parameter provided at the command line");
                throw new Exception("start-app parameter is required");
            }

            applicationMetadata.StartData = startApp;

            // Resolve app ID if provided.
            string appId;
            if (GetValue(StartAppId, out appId))
            {
                applicationMetadata.Id = appId;
            }

            // Resolve instance ID if provided.
            string instId;
            if (GetValue(StartAppInstId, out instId))
            {
                applicationMetadata.InstanceId = instId;
            }

            // Resolve app type if provided.
            string appType;
            if (GetValue(StartAppType, out appType))
            {
                applicationMetadata.AppType = (ApplicationType) Enum.Parse(typeof (ApplicationType), appType, true);
            }

            // Resolve environment, default to Production.
            applicationMetadata.Environment = ApplicationEnvironment.Production;

            string environment;
            if (GetValue(Env, out environment))
            {
                applicationMetadata.Environment = (ApplicationEnvironment) Enum.Parse(typeof (ApplicationEnvironment), environment, true);
            }

            string wdPortStr;
            int wdPortInt;
            if (GetValue(WDPort, out wdPortStr) && int.TryParse(wdPortStr, out wdPortInt))
            {
                applicationMetadata.WDPort = wdPortInt;
            }

            // Look for standalone flag
            applicationMetadata.IsStandalone = HasFlag(Standalone);

            return applicationMetadata;
        }

        public static string CreateCommandLine(ApplicationEnvironment environment, string packageUri, ApplicationType appType, string appId, string instanceId, bool isStandalone)
        {
            var cmdLine = string.Format("--{0}=\"{1}\" --{2}={3} --{4}={5} --{6}={7} --{8}={9}",
                StartApp, packageUri,
                StartAppId, appId,
                StartAppInstId, instanceId,
                StartAppType, appType,
                Env, environment);

            if (isStandalone)
            {
                cmdLine += " --standalone";
            }

            return cmdLine;
        }
    }
}