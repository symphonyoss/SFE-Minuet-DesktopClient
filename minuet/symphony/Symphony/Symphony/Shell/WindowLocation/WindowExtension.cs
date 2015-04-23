using Microsoft.Practices.Unity;

namespace Symphony.Shell.WindowLocation
{
    public class WindowExtension : Extension
    {
        public WindowExtension(IUnityContainer container)
            : base(container)
        {
        }

        protected override void SetupContainer(IUnityContainer container)
        {
            //container.RegisterType<IWindowLocator, WindowLocator>(new ContainerControlledLifetimeManager());
        }
    }
}
