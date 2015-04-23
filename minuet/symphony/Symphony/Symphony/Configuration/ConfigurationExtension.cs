using System;
using System.IO;
using Microsoft.Practices.Unity;
using Symphony.Shell;

namespace Symphony.Configuration
{
    public class ConfigurationExtension : Extension
    {
        public ConfigurationExtension(
            IUnityContainer container)
            : base(container)
        {
        }

        protected override void SetupContainer(IUnityContainer container)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, "Symphony");
            path = Path.Combine(path, "settings.config");

            //var configurationSettings = new ConfigurationSettings(path);
            //configurationSettings.Load();

            //container.RegisterInstance(typeof(IConfigurationSettings), configurationSettings, new ContainerControlledLifetimeManager());
        }
    }
}
