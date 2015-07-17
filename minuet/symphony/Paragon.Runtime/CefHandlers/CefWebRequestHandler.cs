﻿using System;
﻿using Paragon.Plugins;
using Xilium.CefGlue;
using Paragon.Runtime.WPF;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;


namespace Paragon.Runtime
{
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }

        public WindowWrapper(Window window)
        {
            _hwnd = new WindowInteropHelper(window).Handle;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }

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
        protected override bool GetAuthCredentials(CefBrowser browser, CefFrame frame, bool isProxy, string host, int port, string realm, string scheme, CefAuthCallback callback)
        {
            string strFriendlyName = AppDomain.CurrentDomain.FriendlyName;
            Process[] pro = Process.GetProcessesByName(strFriendlyName.Substring(0, strFriendlyName.LastIndexOf('.')));
            System.Windows.Forms.IWin32Window handle = new WindowWrapper(pro[0].MainWindowHandle);

            LoginAuthenticationForm NewLogin = new LoginAuthenticationForm(host);
            WindowInteropHelper wih = new WindowInteropHelper(NewLogin);
            wih.Owner = browser.GetHost().GetWindowHandle(); 

            var Result = NewLogin.ShowDialog();
            switch (Result)
            {
                case true:
                    String userName = NewLogin.UserName;
                    String passwd = NewLogin.Password;
                    callback.Continue(userName, passwd);
                    return true;
                case false:
                    NewLogin.Close();
                    break;
            }
            return false;
        }

    }
}