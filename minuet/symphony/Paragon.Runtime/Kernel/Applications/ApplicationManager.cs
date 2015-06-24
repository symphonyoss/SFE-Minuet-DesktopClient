using System;
using System.Collections.Generic;
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

        private Func<string, string, Stream, Stream, IParagonSplashScreen> _createSplashScreen;
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
            ApplicationFamilyName = string.Empty;
            BrowserLanguage = "en-US";
            SpellCheckingEnabled = true;
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
        /// Applicatio family name, if not empty indicates that there could be more than one paragon application running in this browser process.
        /// </summary>
        public string ApplicationFamilyName { get; private set;  }

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
        public bool SpellCheckingEnabled { get; private set; }
        
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
        public void Initialize(Func<string, string, Stream, Stream, IParagonSplashScreen> createSplashScreen, 
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
            ApplicationFamilyName = manifest.FamilyName ?? string.Empty;
            CacheFolder = Path.Combine(_paragonFolder, string.IsNullOrEmpty(manifest.FamilyName) ? manifest.Id : manifest.FamilyName);
            Environment = metadata.Environment;
            SpellCheckingEnabled = manifest.DisableSpellChecking;

            // Initialize CEF
            using (AutoStopwatch.TimeIt("CEF initialization"))
            {
                ParagonRuntime.Initialize(
                    CacheFolder,
                    null,
                    BrowserLanguage,
                    !SpellCheckingEnabled,
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
            try
            {
                // Create and show the splash screen if needed
                if (cmdLine != null && !cmdLine.HasFlag("no-splash") && _createSplashScreen != null)
                {
                    var stylePart = !string.IsNullOrEmpty(manifest.SplashScreenStyle) ? package.GetPart(manifest.SplashScreenStyle) : null;
                    var styleStream = stylePart != null ? stylePart.GetStream() : null;

                    splash = _createSplashScreen(manifest.Name, manifest.Version, package.GetIcon(), styleStream);
                    splashWindow = (Window)splash;
                    metadata.UpdateLaunchStatus = s =>
                    {
                        if (splash != null && splashWindow != null && splashWindow.IsVisible)
                        {
                            splash.ShowText(s);
                        }
                    };
                    splashWindow.Show();
                }

                // Extract the application arguments from the command line
                Dictionary<string, object> args = null;
                string appArgs;
                if (cmdLine != null && cmdLine.GetValue("args", out appArgs) && _appArgumentParser != null)
                {
                    args = _appArgumentParser(appArgs);
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

            if (manifest.SingleInstance || !string.IsNullOrEmpty(manifest.FamilyName))
            {
                var mutexName = string.Format("{0}:{1}:{2}",
                                    (string.IsNullOrEmpty(manifest.FamilyName) ? manifest.Id : manifest.FamilyName), 
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error parsing command line : {0}", ex.Message),
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            try
            {
                using (AutoStopwatch.TimeIt("Gettting application package"))
                {
                    appPackage = appMetadata.GetApplicationPackage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error parsing manifest file : {0}", ex.Message),
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            return true;
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
                redirectService.LaunchApplication(System.Environment.CommandLine);
            }
        }

        private void HandleLaunchRedirect(string args)
        {
            ApplicationMetadata appMetadata;
            IApplicationPackage appPackage;
            var cmdLine = new ParagonCommandLineParser(args);

            if (ResolveMetadataAndPackage(cmdLine, out appMetadata, out appPackage))
            {
                if (appPackage.Manifest.SingleInstance)
                {
                    bool isSingleInstaneLaunched = false;
                    var app = GetSingleApplicationInstance(appMetadata.Id, appPackage.Manifest.FamilyName, out isSingleInstaneLaunched);
                    if (app != null || isSingleInstaneLaunched)
                    {
                        if (app != null && app.WindowManager.AllWindows.Length > 0)
                        {
                            var w = app.WindowManager.AllWindows[0];
                            w.BringToFront();
                            w.FocusWindow();
                        }
                        return;
                    }
                }
                ParagonRuntime.MainThreadContext.Post(delegate { RunApplicationInternal(cmdLine, appPackage, appMetadata); }, null);
            }
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
        /// <param name="familyName">familyName</param>
        /// <returns></returns>
        private IApplication GetSingleApplicationInstance(string appId, string familyName, out bool isSingleInstaneLaunched)
        {
            IApplication app = null;
            isSingleInstaneLaunched = true;
            lock (Lock)
            {
                app = AllApplicaions.FirstOrDefault((a) => a.Metadata.Id == appId);
                if (app == null && !string.IsNullOrEmpty(familyName))
                {
                    if (_pendingSingleInstanceAppLaunches.Contains(appId))
                    {
                        Logger.Info("An instance of the single instance application '" + appId + "' is already being launched");
                        return null;
                    }
                    isSingleInstaneLaunched = false;
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
            /// Forwar the the launch redirect to Applicatoin Manager
            /// </summary>
            /// <param name="args"></param>
            public void LaunchApplication(string args)
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
