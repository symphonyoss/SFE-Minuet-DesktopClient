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
using System.Linq;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.Kernel.Windowing
{
    public class FileDialogCallback : CefRunFileDialogCallback
    {
        private JavaScriptPluginCallback _callback;

        public string[] SelectedFiles { get; private set; }
        public int SelectedAcceptFilter { get; private set; }

        public FileDialogCallback(JavaScriptPluginCallback callback)
        {
            _callback = callback;
        }

        protected override void OnFileDialogDismissed(int selectedAcceptFilter, string[] filePaths)
        {
            SelectedAcceptFilter = selectedAcceptFilter;
            SelectedFiles = filePaths;
            if (_callback != null)
            {
                _callback(new FileDialogResult{ FilePaths = filePaths, SelectedAcceptFilter = selectedAcceptFilter });
            }
        }
    }

    public class FileDialogResult
    {
        public int SelectedAcceptFilter { get; set; }
        public string[] FilePaths { get; set; }
    }
}
