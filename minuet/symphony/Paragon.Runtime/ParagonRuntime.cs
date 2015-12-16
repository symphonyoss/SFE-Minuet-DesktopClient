using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Paragon.Plugins;
using Paragon.Runtime.Properties;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public static class ParagonRuntime
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private static readonly List<ICefWebBrowser> Browsers = new List<ICefWebBrowser>();
        private static long _cefInitialized;
        private static int _cefInitializing;
        private static CefBrowserApplication _cefApp;
        private static int _initThread;

        /// <summary>
        /// Indicates if initialized
        /// </summary>
        public static bool IsInitialized
        {
            get 
            { 
                return Interlocked.Read(ref _cefInitialized) == 1; 
            }
        }

        public static SynchronizationContext MainThreadContext { get; private set; }

        /// <summary>
        /// Raised before CEF is initialized. This event gives the handler a chance to set certain CEF settings.
        /// This event is raised only once for each browser process.
        /// </summary>
        public static event EventHandler<CefInitializationEventArgs> BeforeCefInitialize;

        public static event EventHandler<RenderProcessInitEventArgs> RenderProcessInitialize;

        public static void Initialize(string cachePath = null,
                                      string paragonPath = null,
                                      string browserLanguage = null,
                                      bool disableSpellChecking = false,
                                      bool ignoreCertificateErrors = false,
                                      bool persistSessionCookies = false)
        {
            if (IsInitialized)
            {
                Logger.Debug("CEF initialization aborted because it is already initialized .");
                return;
            }

            using (AutoStopwatch.TimeIt("Initializing runtime"))
            {
                if (Interlocked.CompareExchange(ref _cefInitializing, 1, 0) == 1)
                {
                    Logger.Debug("CEF initialization aborted because initialization is in progress");
                    return;
                }

                Logger.Debug("CEF is initializing");

                MainThreadContext = SynchronizationContext.Current;

                try
                {
                    var logPath = "cef.log";

                    _initThread = Thread.CurrentThread.ManagedThreadId;
                    if (!string.IsNullOrEmpty(cachePath))
                    {
                        logPath = Path.Combine(cachePath, logPath);
                        cachePath = Path.Combine(cachePath, "cache");
                    }

                    var rendererPath = paragonPath ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "paragon.renderer.exe");
                    if (!File.Exists(rendererPath))
                    {
                        rendererPath = Path.Combine(Environment.CurrentDirectory, "paragon.renderer.exe");
                    }

                    if (!File.Exists(rendererPath))
                    {
                        throw new CefRuntimeException("Unable to determine the path to paragon.renderer.exe");
                    }

                    var binPath = Path.GetDirectoryName(rendererPath);

                    if (!File.Exists(Path.Combine(binPath, "libcef.dll")))
                    {
                        throw new CefRuntimeException("Unable to determine the path to libcef.dll");
                    }

                    var settings = new CefSettings
                    {
                        SingleProcess = false,
                        MultiThreadedMessageLoop = true,
                        IgnoreCertificateErrors = ignoreCertificateErrors,
                        LogSeverity = CefLogSeverity.Default,
                        LogFile = logPath,
                        BrowserSubprocessPath = rendererPath,
                        Locale = browserLanguage,
                        CachePath = cachePath,
                        PersistSessionCookies = persistSessionCookies,
                        ProductVersion = string.Format("Paragon/{0} Chrome/{1}", Assembly.GetExecutingAssembly().GetName().Version, CefRuntime.ChromeVersion)
                        
                    };

                    var argArray = Environment.GetCommandLineArgs();


               
                    List<string> appArgs = new List<string>();
                    appArgs.Add("--process-per-tab");

                    // Pass through Kerberos and proxy parameters.
                    for (int i = 0; i < argArray.Length; i++)
                    {
                        string a = argArray[i];
                        if( a.StartsWith("--auth") ||
                            a.StartsWith("--enable-logging") ||
                            a.StartsWith("--v=") ||
                            a.Contains("proxy"))
                        {
                            appArgs.Add(a);
                        }
                    }
                    var argString = string.Join(",", appArgs.ToArray());
                    Logger.Info("Passing Args to CefApp: " + argString);
                    var args = new CefMainArgs(appArgs.ToArray());

                    _cefApp = new CefBrowserApplication(disableSpellChecking, browserLanguage);
                    _cefApp.RenderProcessInitialize += OnRenderProcessInitialize;
                    
                    if (BeforeCefInitialize != null)
                    {
                        var ea = new CefInitializationEventArgs();
                        BeforeCefInitialize(null, ea);
                        settings.LogFile = ea.LogFile;
                        settings.LogSeverity = ea.LogSeverity;
                        if (ea.RemoteDebuggingPort > 0)
                        {
                            settings.RemoteDebuggingPort = ea.RemoteDebuggingPort;
                        }
                    }

                    CefRuntime.Load();

                    var result = CefRuntime.ExecuteProcess(args, _cefApp, IntPtr.Zero);
                    if (result == -1)
                    {
                        CefRuntime.Initialize(args, settings, _cefApp, IntPtr.Zero);
                        Interlocked.Exchange(ref _cefInitialized, 1);
                        Logger.Info("CEF initialized successfully");
                    }
                    else
                    {
                        throw new CefRuntimeException("CEF initialization failed - CefRuntime.ExecuteProcess return code: " + result);
                    }
                }
                catch (CefRuntimeException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Logger.Error("Error initializing CEF: " + e);
                    throw;
                }
                finally
                {
                    Interlocked.Exchange(ref _cefInitializing, 0);
                    if (!IsInitialized && _cefApp != null)
                    {
                        _cefApp = null;
                    }
                }
            }
        }

        private static void OnRenderProcessInitialize(object sender, RenderProcessInitEventArgs e)
        {
            if (RenderProcessInitialize != null)
            {
                RenderProcessInitialize(null, e);
            }
        }

        public static void Shutdown(string reason)
        {
            SendOrPostCallback cb = s =>
            {
                if (Interlocked.Exchange(ref _cefInitialized, 0) == 1)
                {
                    Logger.Info("CEF will shut down because {0}", reason);
                    var browsers = new List<ICefWebBrowser>(Browsers);
                    Logger.Info("Closing all open browsers.");

                    foreach (var browser in browsers)
                    {
                        try
                        {
                            browser.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Failed to close browser", ex);
                        }
                    }

                    Browsers.Clear();

                    Logger.Info("Shutting down CEF.");
                    _cefApp = null;

                    CefRuntime.Shutdown();
                }
            };

            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            if (currentThreadId != _initThread)
            {
                MainThreadContext.Send(cb, null);
            }
            else
            {
                cb(null);
            }
        }

        public static void AddBrowser(ICefWebBrowser b)
        {
            Browsers.Add(b);
            Logger.Info("There are currently {0} browsers open.", Browsers.Count);
        }

        public static void RemoveBrowser(ICefWebBrowser b)
        {
            if (Browsers.Contains(b))
            {
                Browsers.Remove(b);
            }
        }

        public static TBrowser FindBrowser<TBrowser>(int identifier) where TBrowser : class, ICefWebBrowser
        {
            return Browsers.OfType<TBrowser>().FirstOrDefault(b => b.Identifier == identifier);
        }
    }
}