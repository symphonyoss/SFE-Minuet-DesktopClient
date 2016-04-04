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
using Paragon.Plugins;

namespace Paragon.Runtime.Kernel.Windowing
{
    public sealed class CreateWindowRequest
    {
        private readonly WeakReference _callbackReference;
        private readonly JavaScriptPluginCallback _windowCreatedCallback;
        private AutoStopwatch _stopwatch;

        public CreateWindowRequest(
            string startUrl,
            CreateWindowOptions options,
            JavaScriptPluginCallback windowCreatedCallback,
            string id = null)
        {
            _stopwatch = AutoStopwatch.TimeIt("Creating app window");
            RequestId = id;
            if (string.IsNullOrEmpty(id))
            {
                RequestId = Guid.NewGuid().ToString();
            }
            StartUrl = startUrl;
            Options = options;
            if (windowCreatedCallback != null)
            {
                _callbackReference = new WeakReference(windowCreatedCallback, true);
                _windowCreatedCallback = windowCreatedCallback;
            }
        }

        public string RequestId { get; private set; }

        public string StartUrl { get; private set; }

        public CreateWindowOptions Options { get; private set; }

        public void WindowCreated(IApplicationWindow window)
        {
            if (_callbackReference != null && _callbackReference.IsAlive && _windowCreatedCallback != null)
            {
                _windowCreatedCallback(window);
            }

            _stopwatch.Dispose();
        }
    }
}