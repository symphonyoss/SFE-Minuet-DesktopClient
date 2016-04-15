using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GS.CPD.CEF3.Base;
using GS.CPD.CEF3.Common;
using Xilium.CefGlue;

namespace GS.Paragon.RenderProcess
{
    class CefRendererApplication : CefApp, IDisposable
    {
        CefWebRenderProcessHandler _renderProcessHandler;

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            if (_renderProcessHandler == null)
            {
                _renderProcessHandler = new CefWebRenderProcessHandler(this);
            }
            return _renderProcessHandler;
        }

        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
        }

        class CefWebRenderProcessHandler : CefRenderProcessHandler, IDisposable
        {
            CefRendererApplication _app;

            public CefWebRenderProcessHandler(CefRendererApplication app)
            {
                _app = app;
            }

            protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
            {
                base.OnContextCreated(browser, frame, context);
                _app.OnContextCreated(browser, frame, context);
            }

            protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
            {
                base.OnContextReleased(browser, frame, context);
            }

            protected override void OnRenderThreadCreated(CefListValue extraInfo)
            {
                base.OnRenderThreadCreated(extraInfo);
            }

            protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message)
            {
                return base.OnProcessMessageReceived(browser, sourceProcess, message);
            }

            #region IDisposable Members

            public void Dispose()
            {
                _app = null;
            }

            #endregion
        }
    }
}
