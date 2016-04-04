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
using System.Collections.Generic;
using System.Windows.Documents;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Plugins
{
    public interface IRenderSideMessageRouter : IMessageRouter
    {
        event Action<IPluginContext> OnPluginContextCreated;
        void BrowserDestroyed(CefBrowser browser);
        void ContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context);
        void ContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context);
        void InitializePlugins(CefListValue browserPlugins);
        void LocalCallbackInvoked(LocalRenderCallInfo call, object data, int errorCode, string error);
    }
}