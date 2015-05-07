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

        string GetId();

        string GetTitle();

        void FocusWindow();

        void FullScreenWindow();

        void Minimize();

        void Maximize();

        void Restore();

        void DrawAttention(bool autoclear);

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

        string[] RunFileDialog(FileDialogMode mode, string title, string defaultFileName, string[] acceptTypes);
    }
}