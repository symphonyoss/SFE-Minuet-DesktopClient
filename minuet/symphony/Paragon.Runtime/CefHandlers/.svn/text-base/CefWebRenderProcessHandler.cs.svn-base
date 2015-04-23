using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Paragon.Plugins;
using Paragon.Runtime.PackagedApplication;
using Paragon.Runtime.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal class CefWebRenderProcessHandler : CefRenderProcessHandler, IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly IRenderSideMessageRouter _router;
        private bool _disposed;
        private Dictionary<string, string> _jsExtensions;
        private List<IParagonPlugin> _renderSidePlugins;

        public CefWebRenderProcessHandler(IRenderSideMessageRouter router)
            : this(router, null)
        {
        }

        protected CefWebRenderProcessHandler(IRenderSideMessageRouter router, Dictionary<string, string> jsExtensions)
        {
            _router = router;
            _jsExtensions = jsExtensions;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_renderSidePlugins != null)
            {
                foreach (var plugin in _renderSidePlugins)
                {
                    plugin.Shutdown();
                }
            }

            if (_jsExtensions != null)
            {
                _jsExtensions.Clear();
            }

            _jsExtensions = null;
            if (_router != null)
            {
                _router.Dispose();
            }

            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void OnWebKitInitialized()
        {
            Logger.Info("Webkit initialized");
            if (_jsExtensions != null)
            {
                foreach (var n in _jsExtensions.Keys)
                {
                    var val = _jsExtensions[n];
                    if (!string.IsNullOrEmpty(val))
                    {
                        CefRuntime.RegisterExtension(n, val, null);
                        Logger.Info("Registered JavaScript extension: " + n);
                    }
                }
                _jsExtensions.Clear();
                _jsExtensions = null;
            }
            base.OnWebKitInitialized();
        }

        protected override void OnRenderThreadCreated(CefListValue extraInfo)
        {
            Logger.Info("Render thread created");
            if (extraInfo != null && extraInfo.Count > 0)
            {
                var plugins = extraInfo.GetList(0);
                IApplicationPackage package = null;
                List<IPluginInfo> renderPlugins = new List<IPluginInfo>();
                List<IPluginInfo> renderJsPlugins = new List<IPluginInfo>();
                if (extraInfo.Count > 1)
                {
                    var renderSidePluginData = extraInfo.GetString(1);
                    Logger.Info("Render-side plugin information : " + (renderSidePluginData ?? string.Empty));
                    try
                    {
                        if (!string.IsNullOrEmpty(renderSidePluginData))
                        {
                            RenderSidePluginData renderPluginInfo = JsonConvert.DeserializeObject<RenderSidePluginData>(renderSidePluginData);
                            package = (renderPlugins != null && 
                                       !string.IsNullOrEmpty(renderPluginInfo.PackagePath) && 
                                        renderPluginInfo.Plugins != null && 
                                        renderPluginInfo.Plugins.Count > 0) ? new ApplicationPackage(renderPluginInfo.PackagePath) : null;
                            Logger.Info(string.Format("Found {0} render-side plugins in package {1}",
                                        (renderPlugins != null && renderPluginInfo.Plugins != null) ? renderPluginInfo.Plugins.Count : 0,
                                        package != null ? package.PackageFilePath : string.Empty));
                            if (package != null)
                            {
                                foreach (var pluginInfo in renderPluginInfo.Plugins)
                                {
                                    if (pluginInfo != null && pluginInfo.Assembly.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        renderJsPlugins.Add(pluginInfo);
                                    }
                                    else
                                    {
                                        renderPlugins.Add(pluginInfo);
                                    }
                                }

                                if (renderJsPlugins.Count > 0)
                                {
                                    _jsExtensions = PackagedPluginAssemblyResolver.LoadJavaScriptPlugins(package, renderJsPlugins);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error resolving render-side plugins.", ex);
                    }
                }

                if (plugins != null || (package != null && renderPlugins.Count > 0))
                {
                    Logger.Info("Initializing managed plugins");
                    try
                    {
                        if (package != null && renderPlugins != null && renderPlugins.Count > 0)
                        {
                            _router.OnPluginContextCreated += delegate(IPluginContext context)
                            {
                                _renderSidePlugins = PackagedPluginAssemblyResolver.LoadManagedPlugins(context.PluginManager, package, renderPlugins);
                            };
                        }
                        _router.InitializePlugins(plugins);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error initializing managed plugins.", ex);
                    }
                }

                extraInfo.Clear();
            }

            base.OnRenderThreadCreated(extraInfo);
        }

        protected override void OnBrowserCreated(CefBrowser browser)
        {
            Logger.Info(fmt => fmt("Created Browser {0}", browser.Identifier));
            base.OnBrowserCreated(browser);
        }

        protected override void OnBrowserDestroyed(CefBrowser browser)
        {
            _router.BrowserDestroyed(browser);
            base.OnBrowserDestroyed(browser);
        }

        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            _router.ContextCreated(browser, frame, context);
            base.OnContextCreated(browser, frame, context);
        }

        protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            _router.ContextReleased(browser, frame, context);
            base.OnContextReleased(browser, frame, context);
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message)
        {
            return _router.ProcessCefMessage(browser, message);
        }

        protected override void OnUncaughtException(CefBrowser browser, CefFrame frame, CefV8Context context, CefV8Exception exception, CefV8StackTrace stackTrace)
        {
            Logger.Info(fmt => fmt("UncaughtException in Renderer Browser {0} Frame {1}: {2}",
                browser != null ? browser.Identifier : -1,
                frame != null ? frame.Identifier : -1,
                exception != null ? exception.Message : string.Empty));
            base.OnUncaughtException(browser, frame, context, exception, stackTrace);
        }
    }
}