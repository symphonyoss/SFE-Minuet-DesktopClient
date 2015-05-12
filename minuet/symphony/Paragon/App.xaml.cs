using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Paragon.Plugins;
using Paragon.Runtime.Kernel.Applications;
using Paragon.Properties;
using Paragon.Runtime;
using Paragon.Runtime.Win32;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Paragon
{
    public partial class App : ISingleInstanceApp
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly Dictionary<string, object> _args;
        private ParagonSplashScreen _splash;
        private readonly IApplicationMetadata _appMetadata;
        private readonly IApplicationPackage _appPackage;
        private IApplication _application;
        private readonly string _protocolUri;
        private readonly bool _suppressSplashScreen;

        public App(Dictionary<string, object> args, ApplicationMetadata appMetadata, 
            IApplicationPackage appPackage, bool suppressSplashScreen, string protocolUri)
        {
            _args = args;
            _appMetadata = appMetadata;
            _appPackage = appPackage;
            _suppressSplashScreen = suppressSplashScreen;
            _protocolUri = protocolUri;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                Logger.Info("Minuet starting");

                // Create splash screen, if it is not explicitly disabled
                if (!_suppressSplashScreen)
                {
                    ImageSource imageSource = null;
                    Stream iconStream = _appPackage.GetIcon128();
                    if (iconStream != null)
                    {
                        var icon = new System.Drawing.Icon(iconStream, 128, 128);
                        using (var bmp = icon.ToBitmap())
                        {
                            var hbmp = bmp.GetHbitmap();
                            try
                            {
                                imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            }
                            finally
                            {
                                Win32Api.DeleteObject(hbmp);
                            }
                        }
                    }

                    _splash = new ParagonSplashScreen(_appPackage.Manifest.Name, imageSource, _appPackage.Manifest.Version);
                    _splash.Show();

                    _appMetadata.UpdateLaunchStatus = s =>
                    {
                        if (_splash != null)
                        {
                            _splash.ShowText(s);
                        }
                    };
                }

                // CEF has to be initialized prior to accessing the global cookie manager, etc.
                using (AutoStopwatch.TimeIt("CEF initialization"))
                {
                    var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables(Settings.Default.CacheDirectory), _appPackage.Manifest.Id);
                    ParagonRuntime.Initialize(cachePath, _appPackage.Manifest.SpellCheckLanguage, _appPackage.Manifest.DisableSpellChecking, _appMetadata.Environment == ApplicationEnvironment.Development);
                }

                using (AutoStopwatch.TimeIt("Bootstrapping"))
                {
                    // Create the bootstrapper and launch the app.
                    var bootstrapper = new Bootstrapper();
                    var applicationFactory = bootstrapper.Resolve<ApplicationFactory>();
                    _application = applicationFactory.CreateApplication(_appMetadata, _appPackage, _args);
                }

                _application.Closed += OnAppClosed;

                var stopWatch = AutoStopwatch.TimeIt("Launching");
                _application.Launched += (_, __) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (_splash != null)
                        {
                            _splash.Close(); 
                            _splash = null;
                        }
                    }));

                    stopWatch.Dispose();
                };

                _application.Launch();

                if (!string.IsNullOrEmpty(_protocolUri))
                {
                    _application.OnProtocolInvoke(_protocolUri);
                }
            }
            catch (Exception exc)
            {
                Logger.Info(fmt => fmt("Error starting paragon application : {0}", exc));

                MessageBox.Show("Unable to start:\n\n" + exc.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);

                if (_splash != null)
                {
                    _splash.Close();
                }

                if (_application != null)
                {
                    _application.Close();
                    _application = null;
                }
            }
        }

        private void OnAppClosed(object sender, EventArgs eventArgs)
        {
            Dispatcher.BeginInvoke((new Action(() =>
            {
                try
                {
                    if (_splash != null)
                    {
                        _splash.Close();
                        _splash = null;
                    }

                    ParagonRuntime.Shutdown("Minuet is shutting down");
                    Shutdown();
                }
                catch (Exception ex)
                {
                    Logger.Info(fmt => fmt("Error shutting down Minuet : {0}", ex));
                }
            })));
        }

        public void SignalExternalCommandLineArgs(string args)
        {
            var cmdLine = new ParagonCommandLineParser(args);
            if (_application.WindowManager.AllWindows.Length > 0)
            {
                // TODO: If there is more than one window, need a way to determine which window is the 'main' one.
                _application.WindowManager.AllWindows[0].FocusWindow();
            }

            string uri;
            if (cmdLine.GetValue("url", out uri))
            {
                _application.OnProtocolInvoke(uri);
            }
        }
    }
}