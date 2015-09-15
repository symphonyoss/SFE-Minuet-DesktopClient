using System;
using Paragon.Plugins;
using Paragon.Runtime.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public sealed class CefRenderApplication : CefApp, IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private bool _disposed;
        private CefWebRenderProcessHandler _renderProcessHandler;
        private RenderSideMessageRouter _router;

        public void Dispose()
        {
            Dispose(true);
        }

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            // Prevent a separate gpu-process renderer process from being started.
            commandLine.AppendSwitch("in-process-gpu");

            Logger.Info("{0} process started with commandline arguments : {1}",
                string.IsNullOrEmpty(processType) ? "Browser" : processType, commandLine.ToString());

            base.OnBeforeCommandLineProcessing(processType, commandLine);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Logger.Info("CefRenderApplication disposing");
            if (disposing)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                if (_renderProcessHandler != null)
                {
                    _renderProcessHandler.Dispose();
                    _renderProcessHandler = null;
                }

                if (_router != null)
                {
                    _router.Dispose();
                    _router = null;
                }
            }
        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            if (_renderProcessHandler == null)
            {
                _router = new RenderSideMessageRouter();
                _renderProcessHandler = new CefWebRenderProcessHandler(_router);
            }
            return _renderProcessHandler;
        }
    }
}