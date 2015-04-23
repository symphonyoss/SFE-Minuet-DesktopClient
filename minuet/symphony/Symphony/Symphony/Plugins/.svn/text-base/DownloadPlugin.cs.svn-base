using Paragon.Plugins;
using Symphony.Behaviors;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony.download", IsBrowserSide = true)]
    public class DownloadPlugin : IParagonPlugin
    {
        public void Initialize(IApplication application)
        {
            var applicationLoadBehavior = new ApplicationLoadBehavior();
            applicationLoadBehavior.AttachTo(application);
            applicationLoadBehavior.Subscribe(applicationWindow =>
            {
                var downloadbehaviour = new DownloadBehavior();
                downloadbehaviour.AttachTo(applicationWindow);
            });
        }

        public void Shutdown()
        {
            
        }

        [JavaScriptPluginMember]
        public void Stub()
        {
            
        }
    }
}
