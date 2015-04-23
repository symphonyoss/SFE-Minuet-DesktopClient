using System;
namespace Paragon.Plugins
{
    public class ParagonPlugin : IParagonPlugin
    {
        public IApplication Application { get; private set; }

        public void Initialize(IApplication application)
        {
            Application = application;
            try
            {
                OnInitialize();
            }
            catch (Exception ex)
            {
                if( Application != null && Application.Logger != null )
                    Application.Logger.Error(string.Format("Error starting plugin {0} : ", GetType().AssemblyQualifiedName), ex);
            }
        }

        public void Shutdown()
        {
            OnShutdown();
        }

        protected virtual void OnInitialize()
        {
            // Optional override for sub-classes.
        }

        protected virtual void OnShutdown()
        {
            // Optional override for sub-classes.
        }
    }
}