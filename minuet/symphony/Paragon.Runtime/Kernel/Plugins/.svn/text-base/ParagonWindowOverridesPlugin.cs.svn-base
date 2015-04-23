using Paragon.Plugins;
using Paragon.Runtime.Annotations;

namespace Paragon.Runtime.Kernel.Plugins
{
    [JavaScriptPlugin(IsBrowserSide = true, Name = "window", CallbackThread = CallbackThread.Main)]
    public class ParagonWindowOverridesPlugin : ParagonPlugin
    {
        [JavaScriptPluginMember, UsedImplicitly]
        public void ResizeTo(int width, int height)
        {
            var window = Application.FindWindow(PluginExecutionContext.BrowserIdentifier);
            window.ResizeTo(width, height);
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void MoveTo(int x, int y)
        {
            var window = Application.FindWindow(PluginExecutionContext.BrowserIdentifier);
            window.MoveTo(x, y);
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public void Focus(int x, int y)
        {
            var window = Application.FindWindow(PluginExecutionContext.BrowserIdentifier);
            window.FocusWindow();
        }
    }
}