using Microsoft.Practices.Unity;

namespace Symphony.Shell.WindowManagement
{
    public class WindowManagementExtension : Extension
    {
        public WindowManagementExtension(IUnityContainer container)
            : base(container)
        {
        
        }

        protected override void SetupContainer(IUnityContainer container)
        {
            container
                .RegisterSingleton<IWindowPlacementSettings, WindowPlacementSettings>();
        }
    }
}
