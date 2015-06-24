using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Paragon.Plugins.MessageBus
{
    public class MessageBrokerWatcher
    {
        private const string WatcherMutexName = "__paragon_vortext_broker_mutex__";
        private static readonly object Lock = new object();
        string _brokerLibName;
        string _brokerExeName;
        string _brokerLoggingConfig;
        string _brokerProcessCommandLinePrefix;
        string _brokerLibPath;
        string _brokerExePath;
        string _brokerCommandLine;
        private ILogger _logger;
        private Mutex _monitoringOwnershipMutex;
        private ManualResetEvent _stopSignal;
        private int _brokerPort = MessageBroker.DefaultPort;
        
        public MessageBrokerWatcher(ILogger logger, string brokerLibpath, string brokerExePath)
            : this(logger, "-jar ", brokerLibpath, brokerExePath, string.Empty, MessageBroker.DefaultPort)
        {
        }

        public MessageBrokerWatcher(ILogger logger, string brokerProcessCommandLinePrefix, string brokerLibPath, string brokerExePath)
            : this( logger, brokerProcessCommandLinePrefix, brokerLibPath, brokerExePath, string.Empty, MessageBroker.DefaultPort)
        {
        }

        public MessageBrokerWatcher(ILogger logger, string brokerProcessCommandLinePrefix, string brokerLibPath, string brokerExePath, string brokerLoggingConfig, int brokerPort)
        {
            _logger = logger;
            _brokerExePath = brokerExePath;
            _brokerLibPath = brokerLibPath;
            _brokerProcessCommandLinePrefix = brokerProcessCommandLinePrefix;
            _brokerLoggingConfig = brokerLoggingConfig;
            if( brokerPort != MessageBroker.DefaultPort)
                _brokerPort = brokerPort;
        }

        public bool IsConfigurationValid
        {
            get
            {
                if (string.IsNullOrEmpty(_brokerLibPath) || !File.Exists(_brokerLibPath))
                {
                    _logger.Error(string.Format("Message broker library path '{0}' invalid", _brokerLibPath ?? string.Empty));
                    return false;
                }

                if (string.IsNullOrEmpty(_brokerExePath) || !File.Exists(_brokerExePath))
                {
                    _logger.Error(string.Format("Message broker executable path '{0}' invalid", _brokerExePath ?? string.Empty));
                    return false;
                }

                if (string.IsNullOrEmpty(_brokerExeName))
                {
                    _brokerExeName = Path.GetFileNameWithoutExtension(_brokerExePath);
                }

                if (string.IsNullOrEmpty(_brokerLibName))
                {
                    _brokerLibName = Path.GetFileName(_brokerLibPath);
                }

                return true;
            }
        }

        private string BrokerCommandLine
        {
            get
            {
                if (string.IsNullOrEmpty(_brokerCommandLine))
                {
                    var tempPath = Path.GetTempPath();
                    if( !tempPath.EndsWith("\\") )
                        tempPath += "\\";

                    var tempUri = new Uri(tempPath);
                    
                    _brokerCommandLine = string.Format("-Djava.io.tmpdir=\"{0}\" ", tempUri.PathAndQuery) + 
                                          (_brokerPort != MessageBroker.DefaultPort ? string.Format("-Dparagon.messagebroker.port={0} ", _brokerPort) : string.Empty) +
                                          (!string.IsNullOrEmpty(_brokerLoggingConfig) && File.Exists(_brokerLoggingConfig) ? 
                                                string.Format("-Dlog4j.configuration=\"{0}\" ", new Uri(_brokerLoggingConfig, UriKind.RelativeOrAbsolute).ToString()) : string.Empty) + 
                                          _brokerProcessCommandLinePrefix +
                                          _brokerLibPath;
                }

                return _brokerCommandLine;
            }
        }

        public void Start(Action onStart)
        {
            if (!IsConfigurationValid)
            {
                return;
            }

            bool watchingStarted = false;

            lock (Lock)
            {
                if (_stopSignal == null)
                {
                    new Action(StartInternal).BeginInvoke(null, null);
                    watchingStarted = true;
                }
            }

            if (watchingStarted && onStart != null)
            {
                new Action(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var p = GetMessageBrokerProcess();
                            if (p != null)
                            {
                                onStart();
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Error locating browser process", ex);
                        }
                    }
                }).BeginInvoke(null, null);
            }
        }

        public void Stop()
        {
            lock (Lock)
            {
                if (_stopSignal != null)
                {
                    // Set the stop signal. This should allow the StartMonitoring call to exit gracefully.
                    _stopSignal.Set();
                }
            }
        }

        private void StartInternal()
        {
            // Intialize the synchronization objects
            lock (Lock)
            {
                if (_stopSignal == null)
                {
                    _stopSignal = new ManualResetEvent(false);
                }
                if (_monitoringOwnershipMutex == null)
                {
                    _monitoringOwnershipMutex = new Mutex(false, WatcherMutexName);
                }
            }

            // Wait for monitoring ownership
            if (WaitForMonitoringOwnership())
            {
                _logger.Info("Received signal to monitor broker process. Starting monitoring");
                // Monitor the broker process
                MonitorBrokerProcess();
            }

            // Cleanup the synchronization objects
            lock (Lock)
            {
                if (_stopSignal != null)
                {
                    _stopSignal.Close();
                }
                _stopSignal = null;

                if (_monitoringOwnershipMutex != null)
                {
                    _monitoringOwnershipMutex.Close();
                }
                _monitoringOwnershipMutex = null;
            }
        }

        public Process FindMessageBrokerProcess()
        {
            var procs = IsConfigurationValid ? Process.GetProcessesByName(_brokerExeName) : null;
            return procs.FirstOrDefault(p => IsParagonMessageBrokerProcess(p.Id));
        }

        public Process GetMessageBrokerProcess()
        {
            Process p = null;
            lock (Lock)
            {
                p = FindMessageBrokerProcess();
                if (p == null)
                {
                    _logger.Info("Creating broker process");

                    var psi = new ProcessStartInfo(_brokerExePath, BrokerCommandLine)
                    {
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    p = Process.Start(psi);
                }
            }
            return p;
        }

        private bool IsParagonMessageBrokerProcess(int id)
        {
            try
            {
                var query = new ObjectQuery("SELECT * FROM Win32_Process where ProcessId = " + id);
                var scope = new ManagementScope("\\\\.\\ROOT\\cimv2");

                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var commandLine = obj["CommandLine"].ToString();

                        return !string.IsNullOrEmpty(commandLine)
                               && commandLine.IndexOf(_brokerLibName, StringComparison.InvariantCultureIgnoreCase) >= 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error retrieving the command line for JSI process : {0}", ex.Message);
            }

            return false;
        }

        private void MonitorBrokerProcess()
        {
            var monitoringHandles = new WaitHandle[2];
            monitoringHandles[0] = _stopSignal;
            int relaunchCount = 0;
            int pauseBeforeRelaunch = 0;
            while (true)
            {
                try
                {
                    _logger.Info("Waiting for stop signal or broker process termination");
                    // launch/re-launch/get JSI process
                    monitoringHandles[1] = new ProcessWaitHandle(GetMessageBrokerProcess().Handle);

                    // Wait for stop signal or broker process termination
                    var signaledIndex = WaitHandle.WaitAny(monitoringHandles);

                    // If, stop signal recieved, stop monitoring
                    if (signaledIndex == 0)
                    {
                        _logger.Info("Received stop signal. Stopping the monitoring of broker process.");

                        // Release the monitoring mutex
                        _monitoringOwnershipMutex.ReleaseMutex();
                        return;
                    }

                    // After every 20 relaunches we reset
                    if (relaunchCount > 20)
                    {
                        relaunchCount = 0;
                    }

                    relaunchCount++;
                    // For each relaunch increase the pause time between relaunches by 2 seconds
                    pauseBeforeRelaunch = relaunchCount * 2;

                    _logger.Info(string.Format("The broker process will restart in {0} seconds", pauseBeforeRelaunch));
                    Thread.Sleep(pauseBeforeRelaunch * 1000);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error monitoring broker process. Trying again", ex);
                }
            }
        }

        private bool WaitForMonitoringOwnership()
        {
            var signaledIndex = -1;
            WaitHandle[] waitHandles = { _stopSignal, _monitoringOwnershipMutex };

            while (signaledIndex != 1)
            {
                try
                {
                    // Wait for stop signal or start-monitoring signal
                    _logger.Info("Waiting for monitoring ownership.");
                    signaledIndex = WaitHandle.WaitAny(waitHandles);
                }
                catch (AbandonedMutexException)
                {
                    _logger.Info("The previous process that was monitoring died.");
                    signaledIndex = 1;
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in waiting for monitoting signal.", ex);
                    continue;
                }

                // If, stop signal recieved, stop monitoring
                if (signaledIndex == 0)
                {
                    _logger.Info("Received stop signal. Stopping the monitoring of broker process.");
                    return false;
                }
            }
            // Got monitoring ownership
            return true;
        }

        private class ProcessWaitHandle : WaitHandle
        {
            public ProcessWaitHandle(IntPtr handle)
            {
                SafeWaitHandle = new SafeWaitHandle(handle, false);
            }
        }
    }
}
