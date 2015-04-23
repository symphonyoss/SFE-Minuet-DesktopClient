using System;
using Newtonsoft.Json.Linq;
using Symphony.Configuration;

namespace Symphony.Shell.WindowManagement
{
    public class WindowPlacementSettings : IWindowPlacementSettings
    {
        private const string PlacementNamespace = "window_placement";
        private const string Key = "xml";

        private readonly Func<IConfigurationSettings> getConfigurationSettings;

        public WindowPlacementSettings(Func<IConfigurationSettings> getConfigurationSettings)
        {
            this.getConfigurationSettings = getConfigurationSettings;
        }

        public string GetPlacementAsXml()
        {
            var configurationSettings = this.getConfigurationSettings();
            return configurationSettings.GetValueOrDefault(PlacementNamespace, Key, string.Empty);
        }

        public void Save(string placementAsXml)
        {
            var placementSettings = new JObject();
            placementSettings[Key] = placementAsXml;

            var configurationSettings = this.getConfigurationSettings();
            configurationSettings.Write(PlacementNamespace, placementSettings);
        }
    }
}
