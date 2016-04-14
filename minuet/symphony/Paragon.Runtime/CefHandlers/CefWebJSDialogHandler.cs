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

using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal class CefWebJSDialogHandler : CefJSDialogHandler
    {
        private readonly ICefWebBrowserInternal _core;

        public CefWebJSDialogHandler(ICefWebBrowserInternal core)
        {
            _core = core;
        }

        protected override bool OnJSDialog(CefBrowser browser, string originUrl, string acceptLang, CefJSDialogType dialogType, string messageText, string defaultPromptText, CefJSDialogCallback callback, out bool suppressMessage)
        {
            var ea = new JsDialogEventArgs(dialogType, messageText, defaultPromptText, callback);
            _core.OnJSDialog(ea);
            suppressMessage = ea.Handled && ea.SuppressMessage;
            return ea.Handled;
        }

        protected override bool OnBeforeUnloadDialog(CefBrowser browser, string messageText, bool isReload, CefJSDialogCallback callback)
        {
            var ea = new UnloadDialogEventArgs(messageText, callback);
            _core.OnBeforeUnloadDialog(ea);
            return ea.Handled;
        }

        protected override void OnResetDialogState(CefBrowser browser)
        {
            // Required method override.
        }

        protected override void OnDialogClosed(CefBrowser browser)
        {
            // Required method override.
        }
    }
}