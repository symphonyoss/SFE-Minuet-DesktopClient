using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows;
using Paragon.Plugins;
using Paragon.Properties;
using Paragon.Runtime;
using Paragon.Runtime.Kernel.Applications;
using Paragon.Runtime.Win32;

namespace Paragon
{
    static class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Create command line args parser.
            var cmdLine = new ParagonCommandLineParser(Environment.GetCommandLineArgs());

            /*
             * Following statement is due to the Chromium bug: https://code.google.com/p/chromium/issues/detail?id=125614
             * This statement can be removed once we know for sure that the issue has been fixed by Chrome. 
             * For now, we are disabling any system setting/command line setting for the TZ variable.
             */
            Environment.SetEnvironmentVariable("TZ", null);

            // Launch a debugger if a --debug flag was passed.
            if (cmdLine.HasFlag("debug"))
            {
                Debugger.Launch();
            }

            // Extract app package.
            ApplicationMetadata appMetadata;
            IApplicationPackage appPackage;
            if (!ApplicationManager.ResolveMetadataAndPackage(cmdLine, out appMetadata, out appPackage))
            {
                Environment.ExitCode = 1;
                return;
            }

            try
            {
                ApplicationManager appManager = ApplicationManager.GetInstance();
                
                // Bail if the app is a singleton and an instance is already running. Cmd line args from
                // this instance will be sent to the singleton isntance by the SingleInstance utility.
                if (appManager.RedirectApplicationLaunchIfNeeded(appPackage, appMetadata.Environment))
                {
                    return;
                }

                // Initialize the app.
                App app;
                WorkingSetMonitor workingSetMonitor;

                using (AutoStopwatch.TimeIt("Initializing Paragon.App"))
                {
                    app = new App();
                    app.InitializeComponent();
                    app.Startup += delegate
                    {
                        appManager.Initialize(
                                    (name, version, iconStream, styleStream) =>
                                    {
                                        return new ParagonSplashScreen(name, version, iconStream, styleStream);
                                    },
                                    (package, metadata, args) =>
                                    {
                                        var bootstrapper = new Bootstrapper();
                                        var appFactory = bootstrapper.Resolve<ApplicationFactory>();
                                        return appFactory.CreateApplication(metadata, package, args);
                                    },
                                    (string args) =>
                                    {
                                        var query = HttpUtility.ParseQueryString(args);
                                        return query.Keys.Cast<string>().ToDictionary<string, string, object>(key => key, key => query[key]);
                                    },
                                    Environment.ExpandEnvironmentVariables(Settings.Default.CacheDirectory),
                                    true,
                                    appPackage.Manifest.DisableSpellChecking
                                );

                        appManager.AllApplicationsClosed += delegate
                        {
                            appManager.Shutdown("All applications closed");
                            app.Shutdown();
                        };

                        appManager.RunApplication(cmdLine, appPackage, appMetadata);
                        workingSetMonitor = new WorkingSetMonitor(100, 160);
                    };
                }

                // Run the app (this is a blocking call).
                app.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error launching application : {0}", ex.InnerException != null 
                    ? ex.InnerException.Message : ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.ExitCode = 1;

                throw;
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            ILogger Logger = ParagonLogManager.GetLogger();

            Logger.Error("--exception info start--");
            while (ex != null)
            {
                Logger.Error("exception: " + ex.Message);
                Logger.Error("exception stack trace:" + ex.StackTrace);
                Logger.Error("exception source:" + ex.Source);
                Logger.Error("exception targetSite:" + ex.TargetSite);
                Logger.Error("exception type:" + ex.GetType());
                Logger.Error("exception toString:" + ex.ToString());
                ex = ex.InnerException;
            }
            Logger.Error("--exception info end--");
           
            string errString = "The application will now exit. Please email the following message to your tech support team." + ex.Message + "\n" + ex.StackTrace;
            MessageBox.Show(errString + "\n", "Unhandled Exception", MessageBoxButton.OK);
        }  
    }
}