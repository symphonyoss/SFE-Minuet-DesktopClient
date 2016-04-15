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
using Paragon.Runtime.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public sealed class CefRenderApplication : CefApp, IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private bool _disposed;
        private CefWebRenderProcessHandler _renderProcessHandler;
        private RenderSideMessageRouter _router;

        public void Dispose()
        {
            Dispose(true);
        }

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            // Prevent a separate gpu-process renderer process from being started.
            commandLine.AppendSwitch("in-process-gpu");

            Logger.Info("{0} process started with commandline arguments : {1}",
                string.IsNullOrEmpty(processType) ? "Browser" : processType, commandLine.ToString());

            base.OnBeforeCommandLineProcessing(processType, commandLine);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Logger.Info("CefRenderApplication disposing");
            if (disposing)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                if (_renderProcessHandler != null)
                {
                    _renderProcessHandler.Dispose();
                    _renderProcessHandler = null;
                }

                if (_router != null)
                {
                    _router.Dispose();
                    _router = null;
                }
            }
        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            if (_renderProcessHandler == null)
            {
                _router = new RenderSideMessageRouter();
                _renderProcessHandler = new CefWebRenderProcessHandler(_router);
            }
            return _renderProcessHandler;
        }
    }
}