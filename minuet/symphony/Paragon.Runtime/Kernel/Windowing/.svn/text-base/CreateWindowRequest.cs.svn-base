using System;
using Paragon.Plugins;

namespace Paragon.Runtime.Kernel.Windowing
{
    public sealed class CreateWindowRequest
    {
        private readonly WeakReference _callbackReference;
        private readonly JavaScriptPluginCallback _windowCreatedCallback;
        private AutoStopwatch _stopwatch;

        public CreateWindowRequest(
            string startUrl,
            CreateWindowOptions options,
            JavaScriptPluginCallback windowCreatedCallback,
            string id = null)
        {
            _stopwatch = AutoStopwatch.TimeIt("Creating app window");
            RequestId = id;
            if (string.IsNullOrEmpty(id))
            {
                RequestId = Guid.NewGuid().ToString();
            }
            StartUrl = startUrl;
            Options = options;
            if (windowCreatedCallback != null)
            {
                _callbackReference = new WeakReference(windowCreatedCallback, true);
                _windowCreatedCallback = windowCreatedCallback;
            }
        }

        public string RequestId { get; private set; }

        public string StartUrl { get; private set; }

        public CreateWindowOptions Options { get; private set; }

        public void WindowCreated(IApplicationWindow window)
        {
            if (_callbackReference != null && _callbackReference.IsAlive && _windowCreatedCallback != null)
            {
                _windowCreatedCallback(window);
            }

            _stopwatch.Dispose();
        }
    }
}