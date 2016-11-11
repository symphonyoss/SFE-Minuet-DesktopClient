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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Paragon.Plugins;
using Paragon.Runtime.Properties;
using Paragon.Runtime.Win32;
using System.Diagnostics;
using System.Security;
using System.Windows.Markup;

namespace Paragon.Runtime.Kernel.Applications
{
    /// <summary>
    /// This class manages running paragon applications in the current AppDomain.
    /// </summary>
    /// <remarks>
    /// In a given paragon browser process (paragon.exe), at any time there may be one or more paragon applications running.
    /// All instances of running paragon applications belong to the same "Application Family". 
    /// Any application running may be a "Single Instance" application.
    /// 
    /// If there is more than one application or the only application running is of "Single Instance" type, the application
    /// manager opens an IPC channel to listen to new application launch requests.
    /// </remarks>
    public class ApplicationManager : IApplicationManager, IDisposable
    {
        private static readonly object Lock = new object();
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private static ApplicationManager _instance;
        private readonly List<IApplication> _applications = new List<IApplication>();
        private IpcServerChannel _appManagerChannel;
        private Mutex _mutex;

        private Func<string, string, Stream, IParagonSplashScreen> _createSplashScreen;
        private Func<IApplicationPackage, IApplicationMetadata, Dictionary<string, object>, IApplication> _createApplication;
        private Func<string, Dictionary<string, object>> _appArgumentParser;
        private List<string> _pendingSingleInstanceAppLaunches = new List<string>();

        private string _paragonFolder;

        public event EventHandler AllApplicationsClosed;

        /// <summary>
        /// 'ctor
        /// </summary>
        private ApplicationManager()
        {
            Logger.Info("construct application manager");
            ExplicitShutdown = false;
            ProcessGroup = string.Empty;
            BrowserLanguage = "en-US";
            DisableSpellChecking = false;
        }

        /// <summary>
        /// Returns the singleton instance of this class
        /// </summary>
        /// <returns></returns>
        public static ApplicationManager GetInstance()
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ApplicationManager();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Indicates whether CEF should be shutdown explicitly or should be shutdown when the last application closes.
        /// </summary>
        public bool ExplicitShutdown { get; private set; }
        /// <summary>
        /// Process Group, if not empty indicates that there could be more than one paragon application running in this browser process.
        /// </summary>
        public string ProcessGroup { get; private set;  }

        /// <summary>
        /// Browser Cache folder for all the running applications in this process
        /// </summary>
        public string CacheFolder { get; private set; }

        /// <summary>
        /// Indicates the environment of the running application(s)
        /// </summary>
        public ApplicationEnvironment Environment { get; private set; }

        /// <summary>
        /// The language of the running applications.
        /// </summary>
        public string BrowserLanguage { get; private set; }

        /// <summary>
        /// Indicates whether spell checking is enabled.
        /// </summary>
        public bool DisableSpellChecking { get; private set; }

        public bool EnableMediaStream { get; private set; }
        
        /// <summary>
        /// All the running paragon applications
        /// </summary>
        public IApplication[] AllApplicaions 
        {
            get
            {
                return _applications.ToArray();
            }
        }

        /// <summary>
        /// Indicates whether CEF is initialized
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return ParagonRuntime.IsInitialized;
            }
        }
        
        /// <summary>
        /// Initialize Application Manager
        /// </summary>
        /// <remarks>
        /// This can be called any time before CEF initialize
        /// </remarks>
        /// <param name="createSplshScreen"></param>
        /// <param name="createApplication"></param>
        /// <param name="appArgumentParser"></param>
        /// <param name="paragonFolder"></param>
        /// <param name="explicitShutdown"></param>
        /// <param name="disableSpellChecking"></param>
        public void Initialize(Func<string, string, Stream, IParagonSplashScreen> createSplashScreen, 
                               Func<IApplicationPackage, IApplicationMetadata, Dictionary<string, object>, IApplication> createApplication, 
                               Func<string, Dictionary<string,object>> appArgumentParser,
                               string paragonFolder,
                               bool explicitShutdown = true, 
                               bool disableSpellChecking = false)
        {
            if (IsInitialized)
                throw new Exception("Application manger is already initialized");
            if (createApplication == null)
                throw new Exception("createApplication");
            if (appArgumentParser == null)
                throw new Exception("appArgumentParser");
            if (string.IsNullOrEmpty(paragonFolder))
                throw new ArgumentException("paragonFolder");

            _createApplication = createApplication;
            _appArgumentParser = appArgumentParser;
            _createSplashScreen = createSplashScreen;

            _paragonFolder = paragonFolder;
            ExplicitShutdown = explicitShutdown;
        }

        public void InitializeLogger(string paragonFolder, IApplicationPackage appPackage)
        {
            var cachePath = Path.Combine(paragonFolder, string.IsNullOrEmpty(appPackage.Manifest.ProcessGroup) ? appPackage.Manifest.Id : appPackage.Manifest.ProcessGroup);
            ParagonLogManager.ConfigureLogging(cachePath, LogContext.Browser, Settings.Default.MaxRolledLogFiles);
        }

        public void ShutdownLogger()
        {
            ParagonLogManager.Shutdown();
        }

        /// <summary>
        /// Register an existing application 
        /// </summary>
        /// <param name="app"></param>
        public void Register(IApplication app)
        {
            Logger.Info(string.Format("register application name {0}", app.Name));

            if (!IsInitialized)
                throw new Exception("Application manager is not initialized");

            if (_applications.Find(a => a.Metadata.InstanceId == app.Metadata.InstanceId) != null)
                throw new Exception("Application already registered");

            _applications.Add(app);
            app.Closed += OnAppClosed;
        }

        /// <summary>
        /// Shuts down CEF
        /// </summary>
        /// <param name="reason"></param>
        public void Shutdown(string reason)
        {
            if (ParagonRuntime.IsInitialized)
            {
                ParagonRuntime.Shutdown(reason);
            }
        }

        /// <summary>
        /// Launch the first application in the host process
        /// </summary>
        /// <param name="package"></param>
        /// <param name="metadata"></param>
        public void RunApplication(ParagonCommandLineParser cmdLine, IApplicationPackage package, ApplicationMetadata metadata)
        {
            if (IsInitialized)
                throw new Exception("Application manger is already initialized");

            var manifest = package.Manifest;
    
            // Initialize the following from the first application manifest
            ProcessGroup = manifest.ProcessGroup ?? string.Empty;
            CacheFolder = Path.Combine(_paragonFolder, string.IsNullOrEmpty(manifest.ProcessGroup) ? manifest.Id : manifest.ProcessGroup);
            Environment = metadata.Environment;
            DisableSpellChecking = manifest.DisableSpellChecking;
            EnableMediaStream = manifest.EnableMediaStream;

            // set browser language from manifest - default is en-US
            // "automatic" will set browser language to os culture info
            if (!string.IsNullOrEmpty(manifest.BrowserLanguage))
            {
                if (string.Equals(manifest.BrowserLanguage, "Automatic", StringComparison.InvariantCultureIgnoreCase))
                {
                    BrowserLanguage = CultureInfo.CurrentCulture.Name;
                }
                else
                {
                    //verify specified culture
                    CultureInfo cultureInfo = null;
                    try
                    {
                        cultureInfo = new CultureInfo(manifest.BrowserLanguage);
                    }
                    catch (Exception)
                    {
                        Logger.Error("Manifest browser language is not valid. Using default browser language en-US");
                    }

                    BrowserLanguage = (cultureInfo != null) ? (cultureInfo.Name) : (BrowserLanguage);
                }

                Logger.Info(string.Format("Browser language being used is {0}", BrowserLanguage));
            }

            // Initialize CEF
            using (AutoStopwatch.TimeIt("CEF initialization"))
            {
                ParagonRuntime.Initialize(
                    CacheFolder,
                    null,
                    BrowserLanguage,
                    DisableSpellChecking,
                    EnableMediaStream,
                    Environment == ApplicationEnvironment.Development);
            }

            RunApplicationInternal(cmdLine, package, metadata);
        }

        private void OnAppClosed(object sender, EventArgs e)
        {
            var app = sender as IApplication;
            if (app != null)
            {
                Unregister(app);
            }
        }

        protected virtual void RunApplicationInternal(ParagonCommandLineParser cmdLine, IApplicationPackage package, ApplicationMetadata metadata)
        {
            IParagonSplashScreen splash = null;
            IApplication application = null;
            Window splashWindow = null;
            var manifest = package.Manifest;
#if ENFORCE_PACKAGE_SECURITY
            var isSigned = package.Signature != null;
#endif
            try
            {
                ParagonLogManager.AddApplicationTraceListener(manifest.Id);

                // Load custom WPF theme for the application
                var stylePart = !string.IsNullOrEmpty(manifest.CustomTheme) ? package.GetPart(manifest.CustomTheme) : null;
                var styleStream = stylePart != null ? stylePart.GetStream() : null;
                if (styleStream != null)
                {
                    var theme = XamlReader.Load(styleStream) as ResourceDictionary;
                    if (theme != null)
                    {
#if ENFORCE_PACKAGE_SECURITY
                        if( isSigned )
#endif
                            Application.Current.Resources.MergedDictionaries.Add(theme);
                    }
                }

                // Create and show the splash screen if needed
                if (cmdLine != null && !cmdLine.HasFlag("no-splash") && _createSplashScreen != null)
                {
#if ENFORCE_PACKAGE_SECURITY
                    splash = _createSplashScreen(isSigned ? manifest.Name : manifest.Name + " (Unsigned)", manifest.Version, package.GetIcon());
#else
                    splash = _createSplashScreen(manifest.Name, manifest.Version, package.GetIcon());
#endif
                    splashWindow = (Window)splash;
                    metadata.UpdateLaunchStatus = s =>
                    {
                        if (splash != null && splashWindow != null && splashWindow.IsVisible)
                        {
                            splash.ShowText(s);
                        }
                    };
#if ENFORCE_PACKAGE_SECURITY
                    if (!isSigned)
                        splashWindow.Style = Application.Current.Resources["AlarmSplashScreenStyle"] as Style;
#endif
                    splashWindow.Show();
                }

                // Extract the application arguments from the command line
                Dictionary<string, object> args = null;
                if (cmdLine != null && _appArgumentParser != null)
                {
                    string appArgs, appUrl;
                    if (cmdLine.GetValue("args", out appArgs))
                    {
                        args = _appArgumentParser(appArgs);
                    }
                    else if (cmdLine.GetValue("url", out appUrl))
                    {
                        Uri uri;
                        if (Uri.TryCreate(appUrl, UriKind.Absolute, out uri))
                        {
                            if (!string.IsNullOrEmpty(uri.Query))
                            {
                                args = _appArgumentParser(uri.Query.Substring(1));
                            }
                        }
                    }
                }

                //Create and register application
                application = _createApplication(package, metadata, args);
                Register(application);

                // Launch the application
                var stopWatch = AutoStopwatch.TimeIt("Launching");
                
                application.Launched += delegate
                {
                    if (splashWindow != null)
                    {
                        splashWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (splashWindow != null)
                            {
                                splashWindow.Close();
                            }
                        }));
                    }

                    this.RemoveSingleInstanceLaunchMarker(metadata.Id);

                    stopWatch.Dispose();
                };

                application.Launch();

                string protocolUri = null;
                if (cmdLine != null)
                {
                    cmdLine.GetValue("url", out protocolUri);
                    if (!string.IsNullOrEmpty(protocolUri))
                    {
                        application.OnProtocolInvoke(protocolUri);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info("Error starting paragon application : {0}", ex);

                MessageBox.Show("Unable to start:\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (splashWindow != null)
                {
                    splashWindow.Close();
                }

                if (application != null)
                {
                    RemoveSingleInstanceLaunchMarker(metadata.Id);
                    application.Close();
                    application = null;
                }
            }
        }

        public bool RedirectApplicationLaunchIfNeeded(IApplicationPackage package, ApplicationEnvironment environment)
        {
            bool shouldRedirect = false;
            var manifest = package.Manifest;

            if (manifest.SingleInstance || !string.IsNullOrEmpty(manifest.ProcessGroup))
            {
                var mutexName = string.Format("{0}:{1}:{2}",
                                    (string.IsNullOrEmpty(manifest.ProcessGroup) ? manifest.Id : manifest.ProcessGroup), 
                                    environment, 
                                    System.Environment.UserName);
                string channelName = mutexName + ":LaunchRedirectorService";
                bool createdNew = false;
                var mutex = new Mutex(true, mutexName, out createdNew);
                shouldRedirect = !createdNew;
                if (shouldRedirect)
                {
                    RedirectAplicationLaunch(channelName);
                    mutex.Close();
                }
                else
                {
                    _mutex = mutex;
                    if (!_pendingSingleInstanceAppLaunches.Contains(manifest.Id))
                        _pendingSingleInstanceAppLaunches.Add(manifest.Id);
                    StartApplicationLaunchRedirectionService(channelName);
                }
            }

            return shouldRedirect;
        }

        public static bool ResolveMetadataAndPackage(ParagonCommandLineParser cmdLine, out ApplicationMetadata appMetadata, out IApplicationPackage appPackage)
        {
            appMetadata = null;
            appPackage = null;

            try
            {
                using (AutoStopwatch.TimeIt("Parsing application metadata"))
                {
                    appMetadata = cmdLine.ParseApplicationMetadata();
                    Logger.Info("The current environment is {0}", appMetadata != null ? appMetadata.Environment : ApplicationEnvironment.Production);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error parsing command line : {0}", ex.Message),
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                using (AutoStopwatch.TimeIt("Gettting application package"))
                {
                    appPackage = appMetadata.GetApplicationPackage();
                    Logger.Info("The current application package is {0}", 
                            appPackage == null || appPackage.Signature == null ? "unsigned" : string.Format("digitally signed by {0} on {1}", appPackage.Signature.Signer.Subject, appPackage.Signature.SigningTime));
                    return appPackage != null;
                }
            }
            catch (SecurityException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error parsing manifest file : {0}", ex.Message),
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        private void RedirectAplicationLaunch(string channelName)
        {
            // Send command line args from the new instance to the existing one via remoting.
            var channel = new IpcClientChannel();
            ChannelServices.RegisterChannel(channel, true);
            var serviceUrl = string.Concat("ipc://", channelName, "/", typeof(ApplicationLaunchRedirectionService).Name);

            var redirectService = (ApplicationLaunchRedirectionService)RemotingServices.Connect(typeof(ApplicationLaunchRedirectionService), serviceUrl);

            if (redirectService != null)
            {
                redirectService.LaunchApplication(System.Environment.GetCommandLineArgs());
            }
        }

        private void HandleLaunchRedirect(string[] args)
        {
            var cmdLine = new ParagonCommandLineParser(args);
            ApplicationMetadata appMetadata;
            IApplicationPackage appPackage;
            if (!ResolveMetadataAndPackage(cmdLine, out appMetadata, out appPackage))
            {
                return;
            }

            if (appPackage.Manifest.SingleInstance)
            {
                var isSingleInstanceLaunched = false;
                var app = GetSingleApplicationInstance(appMetadata.Id, appPackage.Manifest.ProcessGroup, out isSingleInstanceLaunched);
                if (app != null || isSingleInstanceLaunched)
                {
                    string protocolUrl;
                    if (app != null && cmdLine.GetValue("url", out protocolUrl))
                    {
                        app.OnProtocolInvoke(protocolUrl);
                    }

                    if (app != null && app.WindowManager.AllWindows.Length > 0)
                    {
                        var w = app.WindowManager.AllWindows[0];
                        w.ShowWindow(false);
                        w.BringToFront();
                        w.ActivateWindow();
                    }

                    return;
                }
            }

            ParagonRuntime.MainThreadContext.Post(delegate { RunApplicationInternal(cmdLine, appPackage, appMetadata); }, null);
        }

        /// <summary>
        /// Unregister an existing application
        /// </summary>
        /// <param name="app"></param>
        private void Unregister(IApplication app)
        {
            Logger.Info(string.Format("unregister application name {0}", app.Name));

            if (_applications.Find(a => a.Metadata.InstanceId == app.Metadata.InstanceId) == null)
            {
                throw new Exception("Application not found");
            }
            _applications.Remove(app);
            if (_applications.Count == 0)
            {
                if (AllApplicationsClosed != null)
                    AllApplicationsClosed(this, EventArgs.Empty);
                if (!ExplicitShutdown)
                {
                    Shutdown("All applications have closed");
                }
            }
        }

        private void StartApplicationLaunchRedirectionService(string channelName)
        {
            var serverProvider = new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full };
            var props = new Dictionary<string, string> { { "name", channelName }, { "portName", channelName }, { "exclusiveAddressUse", "false" } };
            _appManagerChannel = new IpcServerChannel(props, serverProvider);
            ChannelServices.RegisterChannel(_appManagerChannel, true);
            RemotingServices.Marshal(new ApplicationLaunchRedirectionService(), typeof(ApplicationLaunchRedirectionService).Name);
        }

        /// <summary>
        /// Returns the one and only instance of a single instance application
        /// </summary>
        /// <remarks>
        /// If the application is not running it saves marker indicating that the first instance is about to be launched
        /// </remarks>
        /// <param name="appId">applicationId</param>
        /// <param name="processGroup">the name given to a group of applications that run in the same browser process</param>
        /// <param name="isSingleInstanceLaunched"></param>
        /// <returns></returns>
        private IApplication GetSingleApplicationInstance(string appId, string processGroup, out bool isSingleInstanceLaunched)
        {
            IApplication app = null;
            isSingleInstanceLaunched = true;
            lock (Lock)
            {
                app = AllApplicaions.FirstOrDefault((a) => a.Metadata.Id == appId);
                if (app == null && !string.IsNullOrEmpty(processGroup))
                {
                    if (_pendingSingleInstanceAppLaunches.Contains(appId))
                    {
                        Logger.Info("An instance of the single instance application '" + appId + "' is already being launched");
                        return null;
                    }
                    isSingleInstanceLaunched = false;
                    // Add a marker to indicate that the "one and only" instance is being launched
                    _pendingSingleInstanceAppLaunches.Add(appId);
                }
            }
            return app;
        }

        /// <summary>
        /// Removes the marker indicating the launch of the one and only instance of the SingleInstance application
        /// </summary>
        /// <param name="appId"></param>
        private void RemoveSingleInstanceLaunchMarker(string appId)
        {
            lock (Lock)
            {
                if (_pendingSingleInstanceAppLaunches.Contains(appId))
                {
                    _pendingSingleInstanceAppLaunches.Remove(appId);
                }
            }
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            // Release any single instance application mutexes
            if (_mutex != null)
            {
                _mutex.Close();
                _mutex = null;
            }

            // Close the application manager IPC channel, if opened
            if (_appManagerChannel != null)
            {
                ChannelServices.UnregisterChannel(_appManagerChannel);
                _appManagerChannel = null;
            }
        }

        #endregion

        class ApplicationLaunchRedirectionService : MarshalByRefObject
        {
            /// <summary>
            /// Forward the the launch redirect to Application Manager
            /// </summary>
            /// <param name="args"></param>
            public void LaunchApplication(string[] args)
            {
                ApplicationManager.GetInstance().HandleLaunchRedirect(args);
            }

            /// <summary>
            /// Prevents the lease from expiring.
            /// </summary>
            /// <returns></returns>
            public override object InitializeLifetimeService()
            {
                return null;
            }
        }
    }
}
