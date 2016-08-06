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
using Paragon.Runtime.WPF;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Controls;
using System.Windows.Threading;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    internal class CefWebDownloadHandler : CefDownloadHandler
    {
        private readonly ICefWebBrowserInternal _owner;
        Dispatcher _mainUiDispatcher;
        Grid _mainPanel;

        public CefWebDownloadHandler(ICefWebBrowserInternal owner)
        {
            _owner = owner;
            _mainUiDispatcher = Dispatcher.CurrentDispatcher;
        }

        #region downloaderHandler

        bool hasDirWritePerms(string dirPath)
        {
            var writeAllow = false;
            var writeDeny = false;
            var accessControlList = Directory.GetAccessControl(dirPath);
            if (accessControlList == null)
                return false;
            var accessRules = accessControlList.GetAccessRules(true, true,
                                        typeof(System.Security.Principal.SecurityIdentifier));
            if (accessRules == null)
                return false;

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                    continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    writeAllow = true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    writeDeny = true;
            }

            return writeAllow && !writeDeny;
        }

        string getDownLoadFullPath(string fileName)
        {
            string pathUser = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            string pathDownload = Path.Combine(pathUser, "Downloads");

            // fall back to a temp file if directory doesn't exist or doesn't have write permissions
            if (!System.IO.Directory.Exists(pathDownload) || !hasDirWritePerms(pathDownload))
            {
                string newfileName = Path.GetTempFileName();
                string newFullPath = System.IO.Path.ChangeExtension(newfileName, System.IO.Path.GetExtension(fileName));
                return newFullPath;
            }

            return Path.Combine(pathDownload, fileName);
        }

        string GetUniqueFileName(string fullPath)
        {
            string tmpFullPath = fullPath;
            string extn = System.IO.Path.GetExtension(tmpFullPath);
            string fileNameWithoutExtn = Path.GetFileNameWithoutExtension(tmpFullPath);
            string fullPathToFile = System.IO.Path.GetFullPath(fullPath);
            string directoy = System.IO.Path.GetDirectoryName(fullPath);
            int num = 1;
            while (File.Exists(tmpFullPath))
            {
                //tmpFullPath = fullPathToFile + fileNameWithoutExtn + " (" + num + ")" + extn;
                tmpFullPath = System.IO.Path.Combine(directoy,fileNameWithoutExtn) + " (" + num + ")" + extn;
                num++;

                if (num > 20)
                    return tmpFullPath; // give up
            }

            return tmpFullPath;
        }



        protected override void OnBeforeDownload(CefBrowser browser, CefDownloadItem downloadItem, string suggestedName, CefBeforeDownloadCallback callback)
        {
            if (downloadItem.IsValid)
            {

                if (String.IsNullOrEmpty(suggestedName))
                    suggestedName = Path.GetRandomFileName();

                string fullPath = getDownLoadFullPath(suggestedName);

                fullPath = GetUniqueFileName(fullPath);

                suggestedName = Path.GetFileName(fullPath);

                uint id = downloadItem.Id;
                long recvdBytes = downloadItem.ReceivedBytes;
                bool isComplete = downloadItem.IsComplete;
                bool isCanceled = downloadItem.IsCanceled;

                var args = new BeginDownloadEventArgs(downloadItem.Url, downloadItem.MimeType);
                
                args.ID = id;
                args.RecvdBytes = recvdBytes;
                args.IsComplete = isComplete;
                args.IsCanceled = isCanceled;
                args.SuggestedName = suggestedName;
                args.DownloadPath = fullPath;

                _owner.OnBeforeDownload(args);

                callback.Continue(fullPath, false);
            }
        }

        protected override void OnDownloadUpdated(CefBrowser browser, CefDownloadItem downloadItem, CefDownloadItemCallback callback)
        {
            if (downloadItem.IsValid)
            {
                _owner.OnDownloadUpdated(new DownloadUpdatedEventArgs(downloadItem, callback));
            }
        }
        #endregion

/*
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
            _owner.OnBeforeDownload(args);

            if (string.IsNullOrEmpty(args.DownloadPath))
            {
                callback.Continue(string.Empty, true);
            }
            else
            {
                callback.Continue(args.DownloadPath, false);
            }
        }
 */ 
    }
}