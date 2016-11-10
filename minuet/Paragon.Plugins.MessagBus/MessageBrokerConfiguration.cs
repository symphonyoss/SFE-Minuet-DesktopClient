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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Paragon.Plugins.MessageBus
{
    public class MessageBrokerConfiguration
    {
        public const string AppSettingKeyBrokerExePath = "BrokerExePath";
        public const string AppSettingKeyBrokerLibPath = "BrokerLibraryPath";
        public const string AppSettingKeyBrokerPort = "BrokerPort";
        public const string AppSettingKeyBrokerLoggingConfig = "BrokerLoggingConfigFile";

        private readonly string defaultBrokerExePath = Environment.CurrentDirectory;
        private readonly string defaultBrokerLibPath = Path.Combine(Environment.CurrentDirectory, "messagebus.jar");

        private readonly ILogger _logger;

        private string _domain;

        private Configuration _appConfig;
        private Configuration AppConfig
        {
            get
            {
                if (_appConfig == null)
                {
                    _appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                }
                return _appConfig;
            }
        }

        public MessageBrokerConfiguration(ILogger logger)
        {
            _logger = logger;
        }

        public string GetBrokerExePath()
        {
            var brokerExePath = GetSetting(AppConfig, AppSettingKeyBrokerExePath);
            if (string.IsNullOrEmpty(brokerExePath) ||
                brokerExePath.Equals("detect", StringComparison.InvariantCultureIgnoreCase))
            {
                brokerExePath = DetectJavaPath();
            }
            return brokerExePath;
        }

        public string GetBrokerLibPath()
        {
            var brokerLibPath = GetSetting(AppConfig, AppSettingKeyBrokerLibPath);
            if (string.IsNullOrEmpty(brokerLibPath) ||
                brokerLibPath.Equals("detect", StringComparison.InvariantCultureIgnoreCase))
            {
                brokerLibPath = DetectBrokerLibPath();
            }
            else
            {
                brokerLibPath = Path.GetFullPath(brokerLibPath);
            }
            return brokerLibPath;
        }

        public int GetBrokerPort(int defaultPort)
        {
            var brokerPort = GetSetting(AppConfig, AppSettingKeyBrokerPort);
            if (!string.IsNullOrEmpty(brokerPort))
            {
                int portNum;
                if (int.TryParse(brokerPort, out portNum))
                {
                    return portNum;
                }
            }

            return defaultPort;
        }

        public string GetBrokerLoggingConfiguration()
        {
            var brokerLoggingConfig = GetSetting(AppConfig, AppSettingKeyBrokerLoggingConfig);
            if (string.IsNullOrEmpty(brokerLoggingConfig) || brokerLoggingConfig.Equals("detect", StringComparison.InvariantCultureIgnoreCase))
            {
                var file = Path.GetDirectoryName(GetType().Assembly.Location) + "\\messagebroker.log4j.xml";
                brokerLoggingConfig = File.Exists(file) ? file : string.Empty;
            }
            return brokerLoggingConfig;
        }

        private string DetectJavaPath()
        {
            try
            {
                // The java release is 2 POD packages - 32-bit and 64-bit variants.
                // Find the 32-bit version by parsing the 'release' file in the root folder of each POD.
                return Directory.GetDirectories(this.defaultBrokerExePath)
                    .Select(Get32BitJavaPath)
                    .FirstOrDefault(path => !string.IsNullOrEmpty(path));
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to detect Java path", exception);
            }

            return string.Empty;
        }

        private string Get32BitJavaPath(string packageFolder)
        {
            var releasePropertiesFilePaths = Directory.GetFiles(
                packageFolder, "release", SearchOption.TopDirectoryOnly);
            if (releasePropertiesFilePaths.Length != 1)
            {
                _logger.Warn(string.Format("No release file found in Java package [{0}]", packageFolder));
                return null;
            }
            var properties = File.ReadAllLines(releasePropertiesFilePaths[0]);
            if (properties.Any(p => p.StartsWith("OS_ARCH=") && p.Contains("86")))
            {
                // 32-bit Java
                var javaExePath = Path.Combine(packageFolder, "bin\\java.exe");
                if (File.Exists(javaExePath))
                {
                    return javaExePath;
                }
                _logger.Error(string.Format("java.exe not found in [{0}]", packageFolder));
            }
            return null;
        }

        private string DetectBrokerLibPath()
        {
            try
            {
                if (File.Exists(this.defaultBrokerLibPath))
                {
                    return defaultBrokerLibPath;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to detect broker lib path: {0}", ex.Message));
            }

            return string.Empty;
        }

        private static string GetSetting(Configuration config, string key)
        {
            try
            {
                var entry = config != null ? (config.AppSettings.Settings[key]) : (null);
                return entry != null ? (entry.Value) : (null);
            }
            catch
            {
                return null;
            }
        }
    }
}