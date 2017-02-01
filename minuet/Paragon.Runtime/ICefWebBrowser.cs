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
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public interface ICefWebBrowser : IDisposable
    {
        string BrowserName { get; }

        IntPtr BrowserWindowHandle { get; }

        int RenderProcessId { get; }

        /// <summary>
        /// The identifier of the current browser. This value is valid only after the BrowserAfterCreated event is fired.
        /// </summary>
        int Identifier { get; }

        /// <summary>
        /// The URL currently associated with the Main Frame of this browser. Default is 'about:blank'.
        /// </summary>
        string Source { get; set; }

        /// <summary>
        /// Raised before a new browser is created. The handler can modify the Settings, WindowInfo and Client handlers as needed.
        /// It is highly recomended that the client handlers are not modified. Most common usage of this handler is to set the initial
        /// Source URL. The initial source url can be set by setting the Source property of the browser or by calling the LoadApplication,
        /// if the start url is part of a packaged application.
        /// </summary>
        event EventHandler<BrowserCreateEventArgs> BeforeBrowserCreate;

        /// <summary>
        /// Raised before a new popup window is created. The |browser|
        /// and |frame| parameters represent the source of the popup request. The
        /// |target_url| and |target_frame_name| values may be empty if none were
        /// specified with the request. The |popupFeatures| structure contains
        /// information about the requested popup window. To allow creation of the
        /// popup window optionally modify |windowInfo|, |client|, |settings| and
        /// |no_javascript_access| and set Cancel to false. To cancel creation of the popup
        /// window set Cancel to true. The |client| and |settings| values will default to the
        /// source browser's values. The |no_javascript_access| value indicates whether
        /// the new browser window should be scriptable and in the same process as the
        /// source browser.
        /// </summary>
        event EventHandler<BeforePopupEventArgs> BeforePopup;

        /// <summary>
        /// Fired when a new popup must be shown by the handler
        /// </summary>
        event EventHandler<ShowPopupEventArgs> ShowPopup;

        /// <summary>
        /// Raised when the page title changes.
        /// </summary>
        event EventHandler<TitleChangedEventArgs> TitleChanged;

        /// <summary>
        /// Raised when the browser is done loading a frame. The |frame| value will
        /// never be empty -- call the IsMain() method to check if this frame is the
        /// main frame. Multiple frames may be loading at the same time. Sub-frames may
        /// start or continue loading after the main frame load has ended. This method
        /// will always be called for all frames irrespective of whether the request
        /// completes successfully.
        /// </summary>
        event EventHandler<LoadEndEventArgs> LoadEnd;

        /// <summary>
        /// Raised when the render process terminates unexpectedly. |status| indicates how the process terminated.
        /// </summary>
        event EventHandler<RenderProcessTerminatedEventArgs> RenderProcessTerminated;

        /// <summary>
        /// Raised before a context menu is displayed. |params| provides information
        /// about the context menu state. |model| initially contains the default
        /// context menu. The |model| can be cleared to show no context menu or
        /// modified to show a custom menu. Do not keep references to |params| or
        /// |model| outside of this callback.
        /// </summary>
        event EventHandler<ContextMenuCommandEventArgs> ContextMenuCommand;

        /// <summary>
        /// Called to execute a command selected from the context menu. Set Handled true if
        /// the command was handled or false for the default implementation. See
        /// cef_menu_id_t for the command ids that have default implementations. All
        /// user-defined command ids should be between MENU_ID_USER_FIRST and
        /// MENU_ID_USER_LAST. |params| will have the same values as what was passed to
        /// OnBeforeContextMenu(). Do not keep a reference to |params| outside of this
        /// callback.
        /// </summary>
        event EventHandler<ContextMenuEventArgs> BeforeContextMenu;

        event EventHandler<BeginDownloadEventArgs> BeforeDownload;

        /// <summary>
        /// Raised when a download's status or progress information has been updated.
        /// This may be called multiple times before and after OnBeforeDownload().
        /// Execute |callback| either asynchronously or in this method to cancel the
        /// download if desired. Do not keep a reference to |download_item| outside of
        /// this method.
        /// </summary>
        event EventHandler<DownloadProgressEventArgs> DownloadUpdated;

        /// <summary>
        /// Called on the IO thread before a resource request is loaded. The |request|
        /// object may be modified. To cancel the request set Cancel to true otherwise set Cancel to
        /// false.
        /// </summary>
        event EventHandler<ResourceLoadEventArgs> BeforeResourceLoad;

        /// <summary>
        /// Raised to run a JavaScript dialog. The |default_prompt_text| value will be
        /// specified for prompt dialogs only. Set |suppress_message| to true and
        /// set Handled to false to suppress the message (suppressing messages is preferable
        /// to immediately executing the callback as this is used to detect presumably
        /// malicious behavior like spamming alert messages in onbeforeunload). Set
        /// |suppress_message| to false and set Handled to false to use the default
        /// implementation (the default implementation will show one modal dialog at a
        /// time and suppress any additional dialog requests until the displayed dialog
        /// is dismissed). Set Handled to true if the application will use a custom dialog or
        /// if the callback has been executed immediately. Custom dialogs may be either
        /// modal or modeless. If a custom dialog is used the application must execute
        /// |callback| once the custom dialog is dismissed.
        /// </summary>
        event EventHandler<JsDialogEventArgs> JSDialog;

        /// <summary>
        /// Raised to run a dialog asking the user if they want to leave a page. Set Handled to
        /// false to use the default dialog implementation. Set Handled to true if the
        /// application will use a custom dialog or if the callback has been executed
        /// immediately. Custom dialogs may be either modal or modeless. If a custom
        /// dialog is used the application must execute |callback| once the custom
        /// dialog is dismissed.
        /// </summary>
        event EventHandler<UnloadDialogEventArgs> BeforeUnloadDialog;

        /// <summary>
        /// Raised to handle requests for URLs with an unknown
        /// protocol component. Set Allow to true to attempt execution
        /// via the registered OS protocol handler, if any.
        /// SECURITY WARNING: YOU SHOULD USE THIS METHOD TO ENFORCE RESTRICTIONS BASED
        /// ON SCHEME, HOST OR OTHER URL ANALYSIS BEFORE ALLOWING OS EXECUTION.
        /// </summary>
        event EventHandler<ProtocolExecutionEventArgs> ProtocolExecution;

        /// <summary>
        /// Raised after browser is closed
        /// </summary>
        event EventHandler BrowserClosed;

        /// <summary>
        /// Reload the current page.
        /// </summary>
        /// <param name="ignoreCache">When true, the cache will be ignored during the reload.</param>
        void Reload(bool ignoreCache);

        void ExecuteJavaScript(string script);

        /// <summary>
        /// Closes this browser.
        /// </summary>
        void Close(bool force = false);

        /// <summary>
        /// Get an instance of the control that show the developer tools for the current browser
        /// </summary>
        /// <param name="element">Screen point</param>
        /// <param name="info">Window information like size etc. (otional)</param>
        /// <param name="settings">setting (optional)</param>
        /// <returns>A windows forms control. Caller must call the Dispose() method when the containing Form closes.</returns>
        IDisposable GetDeveloperToolsControl(CefPoint element, CefWindowInfo info, CefBrowserSettings settings);

        /// <summary>
        /// Force the creation of underlying control
        /// </summary>
        void CreateControl();

        /// <summary>
        /// Give the focus to the browser
        /// </summary>
        void FocusBrowser();

        /// <summary>
        /// Take the focus away from the browser
        /// </summary>
        void BlurBrowser();

        /// <summary>
        /// Sets/resets the browser's chrome widget window as topmost
        /// </summary>
        /// <param name="set"></param>
        void SetTopMost(bool set);

        void SetZoomLevel(double level);

        void RunFileDialog(CefFileDialogMode mode, string title, string defaultFileName, string[] acceptTypes, int selectedAcceptFilter, CefRunFileDialogCallback callback);

        // if supplied this (not null) this is the initial placement of the window
        Paragon.Runtime.Win32.RECT? initialWindowPlacement { get; }

        void SendKillRenderer();
    }
}