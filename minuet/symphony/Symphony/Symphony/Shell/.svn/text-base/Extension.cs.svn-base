using Microsoft.Practices.Unity;
using Paragon.Plugins;

namespace Symphony.Shell
{
    public class Extension : IExtension
    {
        private readonly IUnityContainer container;

        public Extension(IUnityContainer container)
        {
            this.container = container;
        }

        public virtual void Initalize(IApplication application)
        {
            this.SetupContainer(this.container);
        }

        public virtual void Shutdown()
        {

        }

        protected virtual void SetupContainer(IUnityContainer container)
        {

        }
    }
}
