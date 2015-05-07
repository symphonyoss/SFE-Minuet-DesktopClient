using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xilium.CefGlue;

namespace Paragon.Runtime.Kernel.Windowing
{
    public class FileDialogCallback : CefRunFileDialogCallback
    {
        public string[] SelectedFiles { get; private set; }

        protected override void OnFileDialogDismissed(CefBrowserHost browserHost, string[] filePaths)
        {
            SelectedFiles = filePaths;
        }
    }
}
