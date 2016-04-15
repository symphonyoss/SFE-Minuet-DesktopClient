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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using Paragon.Plugins;
using Paragon.Runtime.PackagedApplication;
using Paragon.Runtime.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal class CefWebRenderProcessHandler : CefRenderProcessHandler, IDisposable
    {
        internal const string RENDER_PROC_ID_MESSAGE = "Paragon.Renderer.ProcessId";

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
            Logger.Info("CefWebRenderProcessHandler disposing");
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
                                        renderPluginInfo.Plugins.Count > 0) ? new ApplicationPackage(renderPluginInfo.PackagePath, p => p) : null;
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
            Logger.Info("Created Browser {0}", browser.Identifier);
            try
            {
                var msg = CefProcessMessage.Create(RENDER_PROC_ID_MESSAGE);
                msg.Arguments.SetInt(0, Process.GetCurrentProcess().Id);
                browser.SendProcessMessage(CefProcessId.Browser, msg);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error notifying the browser {0} it's corresponding render process Id", browser.Identifier), ex);
            }
            base.OnBrowserCreated(browser);
        }

        protected override void OnBrowserDestroyed(CefBrowser browser)
        {
            _router.BrowserDestroyed(browser);
            base.OnBrowserDestroyed(browser);
        }

        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            Logger.Info("Context created");
            _router.ContextCreated(browser, frame, context);
            base.OnContextCreated(browser, frame, context);
        }

        protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            Logger.Info("Context released");
            _router.ContextReleased(browser, frame, context);
            base.OnContextReleased(browser, frame, context);
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message)
        {
            return _router.ProcessCefMessage(browser, message);
        }

        protected override void OnUncaughtException(CefBrowser browser, CefFrame frame, CefV8Context context, CefV8Exception exception, CefV8StackTrace stackTrace)
        {
            Logger.Info("UncaughtException in Renderer Browser {0} Frame {1}: {2}",
                browser != null ? browser.Identifier : -1,
                frame != null ? frame.Identifier : -1,
                exception != null ? exception.Message : string.Empty);
            base.OnUncaughtException(browser, frame, context, exception, stackTrace);
        }
    }
}