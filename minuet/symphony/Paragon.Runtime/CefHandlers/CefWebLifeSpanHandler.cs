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
using Paragon.Runtime.Win32;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal sealed class CefWebLifeSpanHandler : CefLifeSpanHandler
    {
        private static readonly object Lock = new object();
        private ICefWebBrowserInternal _core;

        public CefWebLifeSpanHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override void OnAfterCreated(CefBrowser browser)
        {
            base.OnAfterCreated(browser);

            if (_core != null)
            {
                _core.OnBrowserAfterCreated(browser);
            }
        }

        protected override bool DoClose(CefBrowser browser)
        {
            lock (Lock)
            {
                if (_core != null)
                {
                    Win32Api.SetParent(browser.GetHost().GetWindowHandle(), IntPtr.Zero);
                    OnBeforeClose(browser);
                }

                return false;
            }
        }

        protected override void OnBeforeClose(CefBrowser browser)
        {
            lock (Lock)
            {
                if (_core != null)
                {
                    var core = _core;
                    _core = null;
                    core.OnClosed(browser);
                }
            }
        }

        protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, string targetUrl,
            string targetFrameName, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo,
            ref CefClient client, CefBrowserSettings settings, ref bool noJavascriptAccess)
        {
            var e = new BeforePopupEventArgs(targetUrl, targetFrameName,
                popupFeatures, windowInfo, client, noJavascriptAccess);

            _core.OnBeforePopup(e);
            client = e.Client;
            noJavascriptAccess = e.NoJavascriptAccess;
            return e.Cancel;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _core = null;
            }

            base.Dispose(disposing);
        }
    }
}