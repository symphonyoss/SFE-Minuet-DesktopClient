using System;
using Paragon.Plugins;

namespace Symphony.Behaviors
{
    public class ApplicationLoadBehavior
    {
        private IApplication application;
        private Action<IApplicationWindow> onLoad;

        public void AttachTo(IApplication application)
        {
            this.application = application;
            this.application.WindowManager.CreatedWindow += this.OnWindowCreated;
        }

        public void Subscribe(Action<IApplicationWindow> onLoad)
        {
            this.onLoad = onLoad;
        }

        private void OnWindowCreated(IApplicationWindow applicationWindow, bool isMainWindow)
        {
            if (isMainWindow)
            {
                this.application.WindowManager.CreatedWindow -= this.OnWindowCreated;
                if (this.onLoad != null) this.onLoad(applicationWindow);
            }
        }
    }
}