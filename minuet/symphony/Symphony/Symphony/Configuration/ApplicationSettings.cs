using System;
using Newtonsoft.Json.Linq;

namespace Symphony.Configuration
{
    public class ApplicationSettings
    {
        private const string SystemMenuNamespace = "settings";
        private readonly Func<IConfigurationSettings> getConfigurationSettings;

        public ApplicationSettings(Func<IConfigurationSettings> getConfigurationSettings)
        {
            this.getConfigurationSettings = getConfigurationSettings;
        }

        public bool GetIsMinimizeOnClose()
        {
            var configurationSettings = this.getConfigurationSettings();
            return configurationSettings.GetValueOrDefault(SystemMenuNamespace, "minimizeOnClose", false);
        }

        public void Save(bool isMinimizeOnClose)
        {
            var systemMenuSettings = new JObject();
            systemMenuSettings["minimizeOnClose"] = isMinimizeOnClose;

            var configurationSettings = this.getConfigurationSettings();
            configurationSettings.Write(SystemMenuNamespace, systemMenuSettings);
        }
    }
}
