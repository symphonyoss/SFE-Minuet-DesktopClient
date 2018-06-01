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
using System.Windows.Interop;
using Paragon.Plugins;
using Paragon.Runtime.WPF;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal sealed class CefWebRequestHandler : CefRequestHandler
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly ICefWebBrowserInternal _core;

        public CefWebRequestHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override void OnRenderProcessTerminated(CefBrowser browser, CefTerminationStatus status)
        {
            _core.OnRenderProcessTerminated(new RenderProcessTerminatedEventArgs(browser, status));
        }

        protected override bool OnOpenUrlFromTab(CefBrowser browser, CefFrame frame, string targetUrl, CefWindowOpenDisposition targetDisposition, bool userGesture)
        {
            return _core.OnOpenUrlFromTab(browser, frame, targetUrl, targetDisposition, userGesture);
        }

        protected override CefReturnValue OnBeforeResourceLoad(CefBrowser browser, CefFrame frame, CefRequest request, CefRequestCallback callback)
        {
            var ea = new ResourceLoadEventArgs(request.Url, request.ResourceType, callback);
            _core.OnBeforeResourceLoad(ea);
            return (ea.Cancel ? CefReturnValue.Cancel : CefReturnValue.Continue);
        }

        protected override bool OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool isRedirect)
        {
            return _core.OnBeforeBrowse(browser, frame, request, isRedirect);
        }

        protected override void OnProtocolExecution(CefBrowser browser, string url, out bool allowOSExecution)
        {
            base.OnProtocolExecution(browser, url, out allowOSExecution);
            var ea = new ProtocolExecutionEventArgs(url);
            _core.OnProtocolExecution(ea);
            allowOSExecution = ea.Allow;
        }

        protected override bool OnCertificateError(CefBrowser browser, CefErrorCode certError, string requestUrl, CefSslInfo sslInfo, CefRequestCallback callback)
        {
            // Note that OnCertificateError is only called when the top-level resource (the html page being loaded)
            // has a certificate problem. Any additional resources loaded by the main frame will not trigger this callback.
            Logger.Error("Failed to load resource due to an invalid certificate: " + requestUrl + " with error code: " + certError.ToString());
            Logger.Error("Cert Status: " + sslInfo.CertStatus.ToString());
            if (((sslInfo.CertStatus & CefCertStatus.CTComplianceFailed) != CefCertStatus.None) || 
                ((sslInfo.CertStatus & CefCertStatus.CTCompliancedRequired) != CefCertStatus.None))
            {
                // DES-12985: add command line option to ignore certificate tranparency errors
                string[] args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--ignore-ct-errors")
                    {
                        Logger.Error("Ignoring certificate transparency errors");
                        callback.Continue(true);
                        return true;
                    }
                }
            }
            _core.OnCertificateError();
            return base.OnCertificateError(browser, certError, requestUrl, sslInfo, callback);
        }

        protected override bool GetAuthCredentials(CefBrowser browser, CefFrame frame, bool isProxy, string host, int port, string realm, string scheme, CefAuthCallback callback)
        {
            // Note : this event is fired on the CEF IO Thread. Before showing the auth dialog the call should be marshalled to the UI thread of the browser process
            return _core.OnGetAuthCredentials(browser, frame, isProxy, host, port, realm, scheme, callback);
        }
    }
}