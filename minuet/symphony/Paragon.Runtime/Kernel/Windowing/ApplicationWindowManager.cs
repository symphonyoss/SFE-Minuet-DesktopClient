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
using Paragon.Plugins;
    
namespace Paragon.Runtime.Kernel.Windowing
{
    public class ApplicationWindowManager : IApplicationWindowManagerEx
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly object _lock = new object();
        private readonly List<CreateWindowRequest> _pendingWindowCreations = new List<CreateWindowRequest>();
        private readonly List<IApplicationWindowEx> _windows = new List<IApplicationWindowEx>();
        private Func<ICefWebBrowser> _getRootBrowser;
        private ICefWebBrowser _rootBrowser;
        private Func<IApplicationWindowEx> CreateNewApplicationWindow { get; set; }

        public event EventHandler CreatingWindow;
        public event Action<IApplicationWindow, bool> CreatedWindow;
        public event EventHandler NoWindowsOpen;

        public IApplication Application { get; private set; }

        public IApplicationWindow[] AllWindows
        {
            get
            {
                lock (_lock)
                {
                    return _windows.ToArray();
                }
            }
        }

        public void Initialize(IApplication application, Func<IApplicationWindowEx> createNewWindow, Func<ICefWebBrowser> getRootBrowser)
        {
            if (application == null)
            {
                Logger.Error("Unable to initialize because application is null");
                throw new ArgumentNullException("application");
            }

            if (createNewWindow == null)
            {
                Logger.Error("Unable to initialize because createNewWindow is null");
                throw new ArgumentNullException("createNewWindow");
            }

            if (getRootBrowser == null)
            {
                Logger.Error("Unable to initialize because getRootBrowser is null");
                throw new ArgumentNullException("getRootBrowser");
            }

            Application = application;
            CreateNewApplicationWindow = createNewWindow;
            _getRootBrowser = getRootBrowser;
        }

        public virtual void CreateWindow(CreateWindowRequest request)
        {
            if (request == null)
            {
                Logger.Error("Unable to create window because create window request is null");
                throw new ArgumentNullException("request");
            }

            var browser = GetRootBrowser();
            if (browser != null)
            {
                lock (_pendingWindowCreations)
                {
                    _pendingWindowCreations.Add(request);
                }
                browser.ExecuteJavaScript(string.Format("window.open('{0}', '{1}');", request.StartUrl, request.RequestId));
                Logger.Debug("Created window with URL {0} and ID {1}", request.StartUrl, request.RequestId);
            }
            else
            {
                Logger.Error("Unable to create window because root browser is null");
            }
        }

        public void BeforeApplicationWindowPopup(BeforePopupEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                Logger.Error("Unable to apply settings to popup window. EventArgs is null");
                return;
            }

            // Create pending request based on details supplied to window.open
            var options = new CreateWindowOptions {Id = eventArgs.TargetFrameName};
            if (eventArgs.PopupFeatures != null)
            {
                options.OuterBounds = new BoundsSpecification
                {
                    Left = eventArgs.PopupFeatures.X.HasValue ? eventArgs.PopupFeatures.X.Value : double.NaN,
                    Top = eventArgs.PopupFeatures.Y.HasValue ? eventArgs.PopupFeatures.Y.Value : double.NaN,
                    Height = eventArgs.PopupFeatures.Height.HasValue ? eventArgs.PopupFeatures.Height.Value : double.NaN,
                    Width = eventArgs.PopupFeatures.Width.HasValue ? eventArgs.PopupFeatures.Width.Value : double.NaN
                };

                options.InitialState = "normal";
                if (eventArgs.PopupFeatures.Fullscreen)
                {
                    options.InitialState = "fullscreen";
                }

                options.Resizable = eventArgs.PopupFeatures.Resizable;
                options.Frame = new FrameOptions {Type = Application.Package.Manifest.DefaultFrameType};
                options.AlwaysOnTop = false;
                options.Focused = true;
                options.Hidden = false;
            }
            var request = new CreateWindowRequest(eventArgs.TargetUrl, options, null, eventArgs.TargetFrameName);
            lock (_pendingWindowCreations)
            {
                _pendingWindowCreations.Add(request);
            }
            eventArgs.NeedCustomPopupWindow = true;
            eventArgs.Cancel = false;
        }

        public void ShowApplicationWindowPopup(IApplicationWindowEx applicationWindow, ShowPopupEventArgs eventArgs)
        {
            CreateWindowInternal(eventArgs, request => request.RequestId == eventArgs.PopupBrowser.BrowserName);
            Logger.Debug("Application window with ID {0} shown.", applicationWindow.GetId());
        }

        public void RemoveApplicationWindow(IApplicationWindowEx applicationWindow)
        {
            if (applicationWindow == null)
            {
                Logger.Error("Unable to remove window because applicationWindow is null");
                throw new ArgumentNullException("applicationWindow");
            }

            lock (_lock)
            {
                _windows.Remove(applicationWindow);
            }
            if (_windows.Count == 0)
            {
                NoWindowsOpen.Raise(this, EventArgs.Empty);
            }

            Logger.Debug("Application window with ID {0} removed.", applicationWindow.GetId());
        }

        public void Shutdown()
        {
            lock (_lock)
            {
                _getRootBrowser = null;
                _pendingWindowCreations.Clear();

                foreach (var applicationWindow in _windows)
                {
                    // TODO: Detach events
                    applicationWindow.CloseWindow();
                }
                _windows.Clear();
            }

            if (_rootBrowser != null)
            {
                _rootBrowser.BeforePopup -= OnRootBrowserBeforePopup;
                _rootBrowser.ShowPopup -= OnRootBrowserShowPopup;
                _rootBrowser = null;
            }

            Logger.Debug("Shutdown");
        }

        private ICefWebBrowser GetRootBrowser()
        {
            if (_rootBrowser == null)
            {
                _rootBrowser = _getRootBrowser();
                if (_rootBrowser != null)
                {
                    _rootBrowser.BeforePopup += OnRootBrowserBeforePopup;
                    _rootBrowser.ShowPopup += OnRootBrowserShowPopup;
                }
            }
            return _rootBrowser;
        }

        private void OnRootBrowserBeforePopup(object sender, BeforePopupEventArgs eventArgs)
        {
            // FIXME: Depending on the ordering of an enum seems a bit dodgy to me.
            if (Application.State <= ApplicationState.Running)
            {
                eventArgs.NeedCustomPopupWindow = true;
                eventArgs.Cancel = false;
            }
        }

        private void OnRootBrowserShowPopup(object sender, ShowPopupEventArgs eventArgs)
        {
            CreateWindowInternal(eventArgs, request => request.RequestId == eventArgs.PopupBrowser.BrowserName);
        }

        private void CreateWindowInternal(ShowPopupEventArgs e, Predicate<CreateWindowRequest> requestPredicate)
        {
            var pendingCreationRequest = _pendingWindowCreations.Find(requestPredicate);
            if (pendingCreationRequest == null)
            {
                return;
            }

            CreatingWindow.Raise(this, EventArgs.Empty);

            lock (_pendingWindowCreations)
            {
                _pendingWindowCreations.Remove(pendingCreationRequest);
            }

            var window = CreateWindow(e.PopupBrowser, pendingCreationRequest.StartUrl, pendingCreationRequest.Options);

            if (pendingCreationRequest.Options == null || !pendingCreationRequest.Options.Hidden)
            {
                window.ShowWindow();
            }

            if (CreatedWindow != null)
            {
                CreatedWindow(window, AllWindows.Length == 1);
            }

            pendingCreationRequest.WindowCreated(window);
            e.Shown = true;
        }

        protected virtual IApplicationWindowEx CreateWindow(ICefWebBrowser browser, string startUrl, CreateWindowOptions options)
        {
            var window = CreateNewApplicationWindow();
            
            window.Initialize(this, browser, startUrl, Application.Package.Manifest.Name, options);
            AddWindow(window);
            return window;
        }

        protected void AddWindow(IApplicationWindowEx window)
        {
            lock (_lock)
            {
                _windows.Add(window);
            }
        }
    }
}