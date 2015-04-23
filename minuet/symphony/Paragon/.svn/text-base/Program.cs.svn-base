using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows;
using Paragon.Plugins;
using Paragon.Runtime;
using Paragon.Runtime.Kernel.Applications;
using Paragon.Runtime.Win32;

namespace Paragon
{
    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // Create command line args parser.
            var cmdLine = new ParagonCommandLineParser(Environment.GetCommandLineArgs());

            // Launch a debugger if a --debug flag was passed.
            if (cmdLine.HasFlag("debug"))
            {
                Debugger.Launch();
            }

            // Extract app package.
            ApplicationMetadata appMetadata;
            IApplicationPackage appPackage;
            if (!ResolveMetadataAndPackage(cmdLine, out appMetadata, out appPackage))
            {
                Environment.ExitCode = 1;
                return;
            }

            try
            {
                // Bail if the app is a singleton and an instance is already running. Cmd line args from
                // this instance will be sent to the singleton isntance by the SingleInstance utility.
                if (AppIsSingletonAndInstanceIsAlreadyRunning(appMetadata, appPackage))
                {
                    return;
                }

                // Initialize the app.
                App app;
                using (AutoStopwatch.TimeIt("Initializing Paragon.App"))
                {
                    // Extract any arguments passed via --args at the cmd line.
                    // Args are passed in URL query string format (ex: --args=abc=100&xyz=hello).
                    Dictionary<string, object> args = null;
                    string appArgs;
                    if (cmdLine.GetValue("args", out appArgs))
                    {
                        var query = HttpUtility.ParseQueryString(appArgs);
                        args = query.Keys.Cast<string>().ToDictionary<string, string, object>(key => key, key => query[key]);
                    }

                    // Extract the protocol URL if the app was started from a protocol invocation.
                    string protocolUri;
                    cmdLine.GetValue("url", out protocolUri);

                    // Create and initialize the app.
                    app = new App(args, appMetadata, appPackage, cmdLine.HasFlag("no-splash"), protocolUri);
                    app.InitializeComponent();
                    app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                }

                // Run the app (this is a blocking call).
                app.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error launching application : {0}", ex.InnerException != null 
                    ? ex.InnerException.Message : ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.ExitCode = 1;
            }

            // If the app is a singleton, cleanup the singleton instance.
            if (appPackage != null && appPackage.Manifest != null && appPackage.Manifest.SingleInstance)
            {
                SingleInstance.Cleanup();
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            string errString = "The application will now exit. Please email the following message to your tech support team." + ex.Message + "\n" + ex.StackTrace;
            MessageBox.Show(errString + "\n", "Unhandled Exception", MessageBoxButton.OK);
        }

        private static bool ResolveMetadataAndPackage(ParagonCommandLineParser cmdLine, 
            out ApplicationMetadata appMetadata, out IApplicationPackage appPackage)
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

        private static bool AppIsSingletonAndInstanceIsAlreadyRunning(
            ApplicationMetadata appMetadata, IApplicationPackage appPackage)
        {
            var uniqueAppId = string.Concat(Enum.GetName(typeof(ApplicationEnvironment),
                appMetadata.Environment), ":", appPackage.Manifest.Id);

            using (AutoStopwatch.TimeIt("Checking for single instance constraints"))
            {
                if (appPackage.Manifest.SingleInstance
                    && !SingleInstance.InitializeAsFirstInstance(uniqueAppId))
                {
                    // App is marked as single instance and an instance is already running.
                    return true;
                }
            }

            return false;
        }
    }
}