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

using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal interface ICefWebBrowserInternal
    {
        void OnBrowserAfterCreated(CefBrowser browser);
        void OnBeforeDownload(BeginDownloadEventArgs args);
        void OnBeforePopup(BeforePopupEventArgs args);
        void OnCertificateError();
        void OnTitleChanged(TitleChangedEventArgs args);
        void OnLoadEnd(LoadEndEventArgs args);
        void OnLoadError(LoadErrorEventArgs args);
        void OnLoadStart(LoadStartEventArgs e);
        void OnRenderProcessTerminated(RenderProcessTerminatedEventArgs args);
        void OnBeforeContextMenu(ContextMenuEventArgs args);
        void OnContextMenuCommand(ContextMenuCommandEventArgs args);
        void OnDownloadUpdated(DownloadUpdatedEventArgs args);
        bool OnProcessMessageReceived(CefBrowser browser, CefProcessMessage message);
        bool OnOpenUrlFromTab(CefBrowser browser, CefFrame frame, string targetUrl, CefWindowOpenDisposition targetDisposition, bool userGesture);
        void OnBeforeResourceLoad(ResourceLoadEventArgs ea);
        void OnJSDialog(JsDialogEventArgs ea);
        void OnBeforeUnloadDialog(UnloadDialogEventArgs ea);
        void OnPreviewKeyEvent(CefKeyEvent keyEvent);
        void OnClosed(CefBrowser browser);

        void OnProtocolExecution(ProtocolExecutionEventArgs ea);

        bool OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool isRedirect);

        bool OnGetAuthCredentials(CefBrowser browser, CefFrame frame, bool isProxy, string host, int port, string realm, string scheme, CefAuthCallback callback);
    }
}