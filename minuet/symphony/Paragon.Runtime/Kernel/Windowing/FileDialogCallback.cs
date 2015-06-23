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
