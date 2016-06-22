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

namespace Paragon.Plugins
{
    /// <summary>
    /// This interspace defines JavaScript's perspective of an application window.
    /// It is a dynamic plugin (so doesn't require a name).
    /// </summary>
    public interface IApplicationWindow
    {
        IntPtr Handle { get; }

        event JavaScriptPluginCallback WindowBoundsChanged;

        event JavaScriptPluginCallback WindowFullScreened;

        event JavaScriptPluginCallback WindowMaximized;

        event JavaScriptPluginCallback WindowMinimized;

        event JavaScriptPluginCallback WindowRestored;

        event JavaScriptPluginCallback WindowClosed;

        event JavaScriptPluginCallback PageLoaded;

        event EventHandler LoadComplete;

        event EventHandler<DownloadProgressEventArgs> DownloadProgress;

        event EventHandler<BeginDownloadEventArgs> BeginDownload;

        string GetId();

        string GetTitle();

        void FocusWindow();

        void ActivateWindow();

        void BringToFront();

        void FullScreenWindow();

        void Minimize();

        void Maximize();

        void Restore();

        void DrawAttention(bool autoclear, int maxFlashes, int timeOut);

        /// <summary>
        /// Draw attention to the window.
        /// </summary>
        void ClearAttention();

        /// <summary>
        /// Close the window.
        /// </summary>
        void CloseWindow();

        /// <summary>
        /// Show the window.
        /// </summary>
        void ShowWindow(bool focused = true);

        /// <summary>
        /// Hide the window.
        /// </summary>
        void HideWindow();

        void MoveTo(int x, int y);

        void ResizeTo(int width, int height);

        BoundsSpecification GetInnerBounds();

        BoundsSpecification GetOuterBounds();

        void SetOuterBounds(BoundsSpecification bounds);

        void RefreshWindow(bool ignoreCache = true);

        bool ContainsBrowser(int browserId);

        void ExecuteJavaScript(string script);

        void SetZoomLevel(double level);

        void RunFileDialog(FileDialogMode mode, string title, string defaultFileName, string[] acceptTypes, JavaScriptPluginCallback callback);
    }
}