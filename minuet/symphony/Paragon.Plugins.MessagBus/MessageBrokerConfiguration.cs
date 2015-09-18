using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace Paragon.Plugins.MessageBus
{
    public class MessageBrokerConfiguration
    {
        public const string AppSettingKeyBrokerExePath = "BrokerExePath";
        public const string AppSettingKeyBrokerLibPath = "BrokerLibraryPath";
        public const string AppSettingKeyBrokerPort = "BrokerPort";
        public const string AppSettingKeyBrokerLoggingConfig = "BrokerLoggingConfigFile";

        private const string DefaultBrokerExePath = "\\\\{0}\\appstore\\51\\Production";
        private const string DefaultBrokerLibPath = "\\\\{0}\\appstore\\1188\\Production\\messagebus.jar";
        private const string DefaultBrokerLibPath2 = "\\\\{0}\\appstore\\1188\\Test\\messagebus.jar";

        private readonly ILogger _logger;

        private string _domain;

        public MessageBrokerConfiguration(ILogger logger)
        {
            _logger = logger;
        }

        public string GetBrokerExePath()
        {
            var brokerExePath = GetSetting(AppSettingKeyBrokerExePath);
            if (string.IsNullOrEmpty(brokerExePath) ||
                brokerExePath.Equals("detect", StringComparison.InvariantCultureIgnoreCase))
            {
                brokerExePath = DetectJavaPath();
            }
            return brokerExePath;
        }

        public string GetBrokerLibPath()
        {
            var brokerLibPath = GetSetting(AppSettingKeyBrokerLibPath);
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
            var brokerPort = GetSetting(AppSettingKeyBrokerPort);
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
            var brokerLoggingConfig = GetSetting(AppSettingKeyBrokerLoggingConfig);
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
                var javaReleasePath = string.Format(DefaultBrokerExePath, Domain);

                // The java release is 2 POD packages - 32-bit and 64-bit variants.
                // Find the 32-bit version by parsing the 'release' file in the root folder of each POD.
                return Directory.GetDirectories(javaReleasePath)
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
                // Use the domain of the local machine to figure out DFS share domain
                var defaultBrokerLibPath = string.Format(DefaultBrokerLibPath, Domain);
                if (File.Exists(defaultBrokerLibPath))
                {
                    return defaultBrokerLibPath;
                }

                var defaultBrokerLibPath2 = string.Format(DefaultBrokerLibPath2, Domain);
                if (File.Exists(defaultBrokerLibPath2))
                {
                    return defaultBrokerLibPath2;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to detect broker lib path: {0}", ex.Message));
            }

            return string.Empty;
        }

        private string Domain
        {
            get
            {
                if (string.IsNullOrEmpty(_domain))
                {
                    var hostnameParts = Dns.GetHostEntry("localhost").HostName.Split('.');
                    _domain = hostnameParts.Length > 1 ? hostnameParts[1] : string.Empty;
                }
                return _domain;
            }
        }

        private static string GetSetting(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return null;
            }
        }
    }
}