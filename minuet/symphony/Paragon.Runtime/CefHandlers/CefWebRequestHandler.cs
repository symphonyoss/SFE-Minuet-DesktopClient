﻿using System;
﻿using Paragon.Plugins;
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
            _core.OnCertificateError();
            Logger.Error("Failed to load resource due to an invalid certificate: " + requestUrl);
            return base.OnCertificateError(browser, certError, requestUrl, sslInfo, callback);
        }
    }
}