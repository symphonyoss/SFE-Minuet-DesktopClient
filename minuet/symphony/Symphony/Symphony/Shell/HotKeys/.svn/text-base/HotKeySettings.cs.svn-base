using System;
using System.Windows.Forms;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Symphony.Configuration;

namespace Symphony.Shell.HotKeys
{
    public class HotKeySettings : IHotKeySettings
    {
        private const string HotKeyNamespace = "hotkeys";
        private readonly Func<IConfigurationSettings> getConfigurationSettings;

        public HotKeySettings(Func<IConfigurationSettings> getConfigurationSettings)
        {
            this.getConfigurationSettings = getConfigurationSettings;
        }

        public bool GetIsHotKeyEnabled()
        {
            var configurationSettings = this.getConfigurationSettings();
            return configurationSettings.GetValueOrDefault(
                HotKeyNamespace,
                "bringToFocus.isEnabled", 
                false);
        }

        public ModifierKeys GetModifier()
        {
            var configurationSettings = this.getConfigurationSettings();

            return configurationSettings.GetEnumValueOrDefault(
                HotKeyNamespace, 
                "bringToFocus.modifiers", 
                ModifierKeys.Control | ModifierKeys.Shift);
        }

        public Keys GetKeys()
        {
            var configurationSettings = this.getConfigurationSettings();

            return configurationSettings.ConvertEnumValueOrDefault(
                HotKeyNamespace,
                "bringToFocus.key",
                Keys.S);
        }

        public void Save(bool isHotKeyEnabled, ModifierKeys modifiers, Keys keys)
        {
            var hotKeySettings = new JObject();
            hotKeySettings["bringToFocus.isEnabled"] = isHotKeyEnabled;
            hotKeySettings["bringToFocus.modifiers"] = (int) modifiers;
            hotKeySettings["bringToFocus.key"] = (int) keys;

            var configurationSettings = this.getConfigurationSettings();
            configurationSettings.Write(HotKeyNamespace, hotKeySettings);
        }
    }
}
