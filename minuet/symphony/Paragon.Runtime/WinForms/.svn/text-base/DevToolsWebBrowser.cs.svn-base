using System;
using System.Windows.Forms;
using Paragon.Runtime.Win32;
using Xilium.CefGlue;

namespace Paragon.Runtime.WinForms
{
    internal class DevToolsWebBrowser : Control, IDisposable
    {
        private CefBrowser _browser;
        private DevToolsWebClient _client;

        public DevToolsWebBrowser()
        {
            SetStyle(
                ControlStyles.ContainerControl
                | ControlStyles.ResizeRedraw
                | ControlStyles.FixedWidth
                | ControlStyles.FixedHeight
                | ControlStyles.StandardClick
                | ControlStyles.UserMouse
                | ControlStyles.SupportsTransparentBackColor
                | ControlStyles.StandardDoubleClick
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.CacheText
                | ControlStyles.EnableNotifyMessage
                | ControlStyles.DoubleBuffer
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UseTextForAccessibility
                | ControlStyles.Opaque,
                false);

            SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.Selectable,
                true);
            _client = new DevToolsWebClient(this);
            CreateControl();
        }

        public CefClient Client
        {
            get { return _client; }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }

                _browser = null;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_browser != null)
            {
                ResizeWindow(_browser.GetHost().GetWindowHandle(), Width, Height);
            }
        }

        private static void ResizeWindow(IntPtr handle, int width, int height)
        {
            if (handle != IntPtr.Zero)
            {
                Win32Api.SetWindowPosition(handle, IntPtr.Zero,
                    0, 0, width, height, SWP.NOZORDER | SWP.NOACTIVATE);
            }
        }

        private void OnAfterCreated(CefBrowser browser)
        {
            _browser = browser;
            this.InvokeIfRequired(() => ResizeWindow(browser.GetHost().GetWindowHandle(), Width, Height));
        }

        private void OnBeforeClose(CefBrowser browser)
        {
            this.InvokeIfRequired(Dispose, true);
        }

        private class DevToolsWebClient : CefClient, IDisposable
        {
            private readonly CefLifeSpanHandler _lifeSpanHandler;
            private DevToolsWebBrowser _ctl;

            public DevToolsWebClient(DevToolsWebBrowser ctl)
            {
                _ctl = ctl;
                _lifeSpanHandler = new DefToolsLifeSpanHandler(ctl);
            }

            public void Dispose()
            {
                _ctl = null;
            }

            protected override CefLifeSpanHandler GetLifeSpanHandler()
            {
                return _lifeSpanHandler;
            }

            private class DefToolsLifeSpanHandler : CefLifeSpanHandler, IDisposable
            {
                private DevToolsWebBrowser _ctl;

                public DefToolsLifeSpanHandler(DevToolsWebBrowser ctl)
                {
                    _ctl = ctl;
                }

                public void Dispose()
                {
                    _ctl = null;
                }

                protected override bool DoClose(CefBrowser browser)
                {
                    return false;
                }

                protected override void OnAfterCreated(CefBrowser browser)
                {
                    _ctl.OnAfterCreated(browser);
                }

                protected override void OnBeforeClose(CefBrowser browser)
                {
                    _ctl.OnBeforeClose(browser);
                    _ctl = null;
                }
            }
        }
    }
}