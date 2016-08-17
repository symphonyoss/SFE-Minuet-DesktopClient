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
using System.IO;
using System.Linq;
using System.Web;
using System.Windows;
using Paragon.Plugins;
using Paragon.Properties;
using Paragon.Runtime;
using Paragon.Runtime.Kernel.Applications;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;
using Paragon.Runtime.Win32;
using Paragon.Runtime.Desktop;

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

        private static Dictionary<string, string> _contentTypesGiven = new Dictionary<string, string>();

        private static Uri GetRelativeUri(string currentFile)
        {
            var uriString = currentFile;
            if (!uriString.StartsWith("\\"))
            {
                uriString = "\\" + uriString;
            }

            var relPath = uriString.Substring(
                uriString.IndexOf('\\')).Replace('\\', '/').Replace(' ', '_');

            var normalized = relPath.Normalize(NormalizationForm.FormKD);

            var removal = Encoding.GetEncoding(
                Encoding.ASCII.CodePage,
                new EncoderReplacementFallback(string.Empty),
                new DecoderReplacementFallback(string.Empty));

            var bytes = removal.GetBytes(normalized);
            var uri = Encoding.ASCII.GetString(bytes);
            return new Uri(uri, UriKind.Relative);
        }


        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write (buffer, 0, read);
            }
        }

        private static void AddFilesToZip(string zipFilename, string[] filesToAdd, CompressionOption compression = CompressionOption.Maximum)
        {
            using (var memStream = new MemoryStream())
            {
                using (Package zip = System.IO.Packaging.Package.Open(memStream, FileMode.OpenOrCreate))
                {
                    foreach (string fileToAdd in filesToAdd)
                    {
                        //skip zip files.
                        if (fileToAdd.EndsWith(".zip"))
                            continue;

                        string destFilename = ".\\" + Path.GetFileName(fileToAdd);
                        Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                        if (zip.PartExists(uri))
                        {
                            zip.DeletePart(uri);
                        }
                        PackagePart part = zip.CreatePart(uri, "", compression);
                        try
                        {
                            using (FileStream fileStream = new FileStream(fileToAdd, FileMode.Open, FileAccess.Read))
                            {
                                using (Stream dest = part.GetStream())
                                {
                                    CopyStream(fileStream, part.GetStream());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                    using (FileStream zipfilestream = new FileStream(zipFilename, FileMode.Create, FileAccess.Write))
                    {
                        memStream.Position = 0;
                        CopyStream(memStream, zipfilestream);
                    }
                }
            }
        }              

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = ParagonLogManager.GetLogger();
            logger.Error(string.Format("An unhandled domain exception has occurred: {0}", e.ExceptionObject ?? string.Empty));
            
            var errMessage = "A fatal error has occurred.";

            try
            {
                String dstDiretory = ParagonLogManager.LogDirectory;
                String fileName = "dump-file-" + DateTime.Now.ToString("dd_MM_yyyy_HH-mm-ss-fff") + ".zip";
                String fullPath = Path.Combine(dstDiretory, fileName);

                //Dump apps.
                WebApplication runningApp = (WebApplication)_appManager.AllApplicaions.FirstOrDefault();
                AppInfo appInfo = (AppInfo)runningApp.GetRunningApps().FirstOrDefault();

                //Dump paragon.exe.
                Process process = Process.GetProcessById(appInfo.BrowserInfo.Pid);// Process.GetProcessesByName("Paragon").First();
                String fileToDump = Path.Combine(dstDiretory, process.ProcessName+".dmp");
                MemoryDump.MiniDumpToFile(fileToDump, process);
                
                //Dump Paragon.Renderer.
                process = Process.GetProcessById(appInfo.RenderInfo.Pid);
                fileToDump = Path.Combine(dstDiretory, process.ProcessName + ".dmp");
                MemoryDump.MiniDumpToFile(fileToDump, process);
               
                String packagePath = Path.Combine(dstDiretory, "package");
                
                //delete temp directory. Ensure cleanup.
                if (System.IO.Directory.Exists(packagePath))
                    System.IO.Directory.Delete(packagePath, true);

                //Create temp directory
                System.IO.Directory.CreateDirectory(packagePath);

                //Copy files to temp directory to avoid denied access to files with open handler.
                string[] fileEntries = Directory.GetFiles(dstDiretory);
                System.Collections.Generic.List<string> files = fileEntries.ToList();
                foreach (string file in fileEntries)
                {
                    if (!file.EndsWith(".zip"))
                    {
                        File.Copy(file, Path.Combine(packagePath, Path.GetFileName(file)));
                    }
                }

                //Zip files.
                fileEntries = Directory.GetFiles(packagePath);
                AddFilesToZip(fullPath, fileEntries);

                errMessage += string.Format("\n\nLog files and application dump files are zipped in:\n{0}.", fullPath);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Failed to collect log files & dump running apps: {0}", e.ExceptionObject ?? string.Empty));
                logger.Error(string.Format("Exception when collecting logs: {0}", ex.ToString()));
            }

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