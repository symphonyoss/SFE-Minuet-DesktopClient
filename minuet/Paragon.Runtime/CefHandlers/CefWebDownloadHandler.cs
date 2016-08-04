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
    internal class CefWebDownloadHandler : CefDownloadHandler
    {
        private readonly ICefWebBrowserInternal _owner;

        public CefWebDownloadHandler(ICefWebBrowserInternal owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Called when a download's status or progress information has been updated.
        /// This may be called multiple times before and after OnBeforeDownload().
        /// Execute |callback| either asynchronously or in this method to cancel the
        /// download if desired. Do not keep a reference to |download_item| outside of
        /// this method.
        /// </summary>
        protected override void OnDownloadUpdated(CefBrowser browser, CefDownloadItem downloadItem, CefDownloadItemCallback callback)
        {
            _owner.OnDownloadUpdated(new DownloadUpdatedEventArgs(downloadItem, callback));
        }

        protected override void OnBeforeDownload(CefBrowser browser, CefDownloadItem downloadItem, string suggestedName, CefBeforeDownloadCallback callback)
        {
            var args = new BeginDownloadEventArgs(downloadItem.Url, downloadItem.MimeType);
            
            args.Id = downloadItem.Id;
            args.SuggestedName = suggestedName;
            args.IsValid = downloadItem.IsValid;
            args.RecvdBytes = downloadItem.ReceivedBytes;
            args.IsComplete = downloadItem.IsComplete;
            args.IsCanceled = downloadItem.IsCanceled;

            _owner.OnBeforeDownload(args);

            callback.Continue(args.DownloadPath, false);

            /*
            if (string.IsNullOrEmpty(args.DownloadPath))
            {
                callback.Continue(string.Empty, true);
            }
            else
            {
                callback.Continue(args.DownloadPath, false);
            }
            */ 
        }
    }
}