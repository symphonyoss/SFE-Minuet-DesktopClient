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
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows;
using Paragon.Plugins;
using Paragon.Properties;
using Paragon.Runtime;
using Paragon.Runtime.Kernel.Applications;

namespace Paragon
{
    static class Program
    {
        private static ApplicationManager _appManager;

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
                _appManager = ApplicationManager.GetInstance();

                //initialize logger earlier in the start up sequence
                _appManager.InitializeLogger(Environment.ExpandEnvironmentVariables(Settings.Default.CacheDirectory), appPackage);

                // Bail if the app is a singleton and an instance is already running. Cmd line args from
                // this instance will be sent to the singleton isntance by the SingleInstance utility.
                if (_appManager.RedirectApplicationLaunchIfNeeded(appPackage, appMetadata.Environment))
                {
                    return;
                }

                // Initialize the app.
                App app;

                using (AutoStopwatch.TimeIt("Initializing Paragon.App"))
                {
                    app = new App();
                    app.InitializeComponent();
                    app.Startup += delegate
                    {
                        _appManager.Initialize(
                                    (name, version, iconStream) => new ParagonSplashScreen(name, version, iconStream),
                                    (package, metadata, args) =>
                                    {
                                        var bootstrapper = new Bootstrapper();
                                        var appFactory = bootstrapper.Resolve<ApplicationFactory>();
                                        return appFactory.CreateApplication(metadata, package, args);
                                    },
                                    (args) =>
                                    {
                                        var query = HttpUtility.ParseQueryString(args);
                                        return query.Keys.Cast<string>().ToDictionary<string, string, object>(key => key, key => query[key]);
                                    },
                                    Environment.ExpandEnvironmentVariables(Settings.Default.CacheDirectory),
                                    true,
                                    appPackage.Manifest.DisableSpellChecking
                                );

                        _appManager.AllApplicationsClosed += delegate
                        {
                            _appManager.Shutdown("All applications closed");
                            _appManager.ShutdownLogger();
                            app.Shutdown();
                        };

                        _appManager.RunApplication(cmdLine, appPackage, appMetadata);
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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = ParagonLogManager.GetLogger();
            logger.Error(string.Format("An unhandled domain exception has occurred: {0}", e.ExceptionObject ?? string.Empty));

            var errMessage = "A fatal error has occurred.";
            if (_appManager != null && _appManager.AllApplicaions != null)
            {
                var apps = _appManager.AllApplicaions.Select(app => app.Name).ToArray();
                if (apps.Length > 0)
                {
                    errMessage += "\n\nThe following applications will be closed: \n    " + string.Join(", ", apps);
                }
            }

            errMessage += "\n\nPlease contact Technology Client Services if you require assistance.\n";
            MessageBox.Show(errMessage, "Unhandled Exception", MessageBoxButton.OK);
        }
    }
}