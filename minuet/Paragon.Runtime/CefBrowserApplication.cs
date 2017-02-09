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
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public sealed class CefBrowserApplication : CefApp
    {
        public static string[] AllowedProtocols = new string[] { "http", "https", "ws", "wss", "blob", "data" };
        private CefWebBrowserProcessHandler _browserProcessHandler;
        public event EventHandler<RenderProcessInitEventArgs> RenderProcessInitialize;
        private readonly bool _disableSpellChecking = false;
        private readonly string _spellCheckLanguage = string.Empty;
        private readonly bool _enableMediaStream = false;

        public CefBrowserApplication()
            : this(false, string.Empty, false)
        {
        }

        public CefBrowserApplication(bool disableSpellChecking, string spellCheckLanguage, bool enableMediaStream)
        {
            _disableSpellChecking = disableSpellChecking;
            _spellCheckLanguage = spellCheckLanguage;
            _enableMediaStream = enableMediaStream;
        }

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            // Prevent a separate gpu-process renderer process from being started.
            commandLine.AppendSwitch("--in-process-gpu");
            
            if (_enableMediaStream)
                commandLine.AppendSwitch("--enable-media-stream");

            base.OnBeforeCommandLineProcessing(processType, commandLine);
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