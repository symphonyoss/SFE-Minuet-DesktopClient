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
using System.ComponentModel;
using Paragon.Plugins.ScreenCapture.Annotations;

namespace Paragon.Plugins.ScreenCapture
{
    [JavaScriptPlugin(Name = "paragon.snippets", IsBrowserSide = true), UsedImplicitly]
    public class ParagonScreenCapturePlugin : ParagonPlugin
    {
        private JavaScriptPluginCallback onComplete;

        [JavaScriptPluginMember, UsedImplicitly]
        public void Capture(JavaScriptPluginCallback onComplete)
        {
            this.onComplete = onComplete;

            var activeWindow = Application.FindWindow(PluginExecutionContext.BrowserIdentifier);
            var window = activeWindow.Unwrap();

            Action toInvoke = () =>
            {
                var snippingWindow = new SnippingWindow();
                snippingWindow.Closing += OnClosing;
                snippingWindow.Owner = window;
                snippingWindow.Show();
            };

            window.Dispatcher.Invoke(toInvoke);
        }

        private void OnClosing(object sender, CancelEventArgs args)
        {
            var snippingWindow = (SnippingWindow) sender;

            var snippet = snippingWindow.Snippet;
            if (snippet != null)
            {
                onComplete(snippet);
            }

            snippingWindow.Closing -= OnClosing;
        }
    }
}