using System;

namespace Paragon.Plugins
{
    /// <summary>
    /// Defines the creation and management of windows for a single hosted or packaged application.
    /// </summary>
    /// <remarks>
    /// From https://developer.chrome.com/apps/app_lifecycle:
    /// An event page may create one or more windows at its discretion.
    /// By default, these windows are created with a script connection to the event page
    /// and are directly scriptable by the event page.
    ///
    /// Windows in Chrome Apps are not associated with any Chrome browser windows.
    /// They have an optional frame with title bar and size controls, and a recommended window ID.
    /// Windows without IDs will not restore to their size and location after restart.
    /// </remarks>
    public interface IApplicationWindowManager
    {
        IApplication Application { get; }
        IApplicationWindow[] AllWindows { get; }
        event EventHandler CreatingWindow;
        event Action<IApplicationWindow, bool> CreatedWindow;
        event EventHandler NoWindowsOpen;
    }
}