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
    public class JsDialogEventArgs : EventArgs
    {
        internal JsDialogEventArgs(CefJSDialogType dialogType, string messageText, string defaultPromptText, CefJSDialogCallback callback)
        {
            DialogType = dialogType;
            MessageText = messageText;
            DefaultPromptText = defaultPromptText;
            Callback = callback;
            SuppressMessage = false;
            Handled = false;
        }

        public CefJSDialogType DialogType { get; private set; }
        public string MessageText { get; private set; }
        public string DefaultPromptText { get; private set; }
        public CefJSDialogCallback Callback { get; private set; }
        public bool SuppressMessage { get; set; }
        public bool Handled { get; set; }
    }
}