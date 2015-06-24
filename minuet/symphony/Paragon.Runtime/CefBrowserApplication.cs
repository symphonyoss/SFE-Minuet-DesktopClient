using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public sealed class CefBrowserApplication : CefApp
    {
        public static string[] AllowedProtocols = new string[] { "http", "https", "ws", "wss", "blob" };
        private CefWebBrowserProcessHandler _browserProcessHandler;
        public event EventHandler<RenderProcessInitEventArgs> RenderProcessInitialize;
        private readonly bool _disableSpellChecking = false;
        private readonly string _spellCheckLanguage = string.Empty;

        public CefBrowserApplication()
            : this(false, string.Empty)
        {
        }

        public CefBrowserApplication(bool disableSpellChecking, string spellCheckLanguage)
        {
            _disableSpellChecking = disableSpellChecking;
            _spellCheckLanguage = spellCheckLanguage;
        }

        protected override CefBrowserProcessHandler GetBrowserProcessHandler()
        {
            if (_browserProcessHandler == null)
            {
                _browserProcessHandler = new CefWebBrowserProcessHandler(_disableSpellChecking, _spellCheckLanguage);
                _browserProcessHandler.RenderProcessInitialize += OnRenderProcessInitialize;
            }
            return _browserProcessHandler;
        }

        private void OnRenderProcessInitialize(object sender, RenderProcessInitEventArgs e)
        {
            if (RenderProcessInitialize != null)
            {
                RenderProcessInitialize(this, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_browserProcessHandler != null)
                {
                    _browserProcessHandler.RenderProcessInitialize -= OnRenderProcessInitialize;
                }

                _browserProcessHandler = null;
            }
            base.Dispose(disposing);
        }
    }

    public class CefWebBrowserProcessHandler : CefBrowserProcessHandler
    {
        public event EventHandler<RenderProcessInitEventArgs> RenderProcessInitialize;
        private readonly bool _disableSpellChecking = false;
        private readonly string _spellCheckLanguage = string.Empty;

        public CefWebBrowserProcessHandler()
            : this(false, string.Empty)
        {
        }

        public CefWebBrowserProcessHandler(bool disableSpellChecking, string spellCheckLanguage)
        {
            _disableSpellChecking = disableSpellChecking;
            _spellCheckLanguage = spellCheckLanguage;
        }

        protected override void OnContextInitialized()
        {
            CefCookieManager.GetGlobal(null).SetSupportedSchemes(CefBrowserApplication.AllowedProtocols, null);
            base.OnContextInitialized();
        }

        protected override void OnRenderProcessThreadCreated(CefListValue extraInfo)
        {
            if (RenderProcessInitialize != null)
            {
                using (var ea = new RenderProcessInitEventArgs(extraInfo))
                {
                    RenderProcessInitialize(this, ea);
                }
            }
            base.OnRenderProcessThreadCreated(extraInfo);
        }

        protected override void OnBeforeChildProcessLaunch(CefCommandLine commandLine)
        {
            if (commandLine.GetSwitchValue("type").StartsWith("render"))
            {
                if (_disableSpellChecking)
                    commandLine.AppendArgument("--disable-spell-checking");
                else if (!string.IsNullOrEmpty(_spellCheckLanguage) && !_spellCheckLanguage.Equals("en-US"))
                    commandLine.AppendArgument(string.Format("--override-spell-check-lang={0}", _spellCheckLanguage));
            }

            base.OnBeforeChildProcessLaunch(commandLine);
        }
    }
}