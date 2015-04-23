using System;
using System.ComponentModel;
using Paragon.Plugins.ScreenCapture.Annotations;

namespace Paragon.Plugins.ScreenCapture
{
    [JavaScriptPlugin(Name = "paragon.snippets", IsBrowserSide = true), UsedImplicitly]
    public class ParagonScreenCapturePlugin : ParagonPlugin
    {
        private JavaScriptPluginCallback onComplete;

        [JavaScriptPluginMember, UsedImplicitly]
        public void Capture(JavaScriptPluginCallback onComplete)
        {
            this.onComplete = onComplete;

            var activeWindow = Application.FindWindow(PluginExecutionContext.BrowserIdentifier);
            var window = activeWindow.Unwrap();

            Action toInvoke = () =>
            {
                var snippingWindow = new SnippingWindow();
                snippingWindow.Closing += OnClosing;
                snippingWindow.Owner = window;
                snippingWindow.Show();
            };

            window.Dispatcher.Invoke(toInvoke);
        }

        private void OnClosing(object sender, CancelEventArgs args)
        {
            var snippingWindow = (SnippingWindow) sender;

            var snippet = snippingWindow.Snippet;
            if (snippet != null)
            {
                onComplete(snippet);
            }

            snippingWindow.Closing -= OnClosing;
        }
    }
}