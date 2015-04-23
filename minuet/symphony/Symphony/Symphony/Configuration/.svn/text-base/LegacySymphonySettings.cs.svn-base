using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using Symphony.Configuration.Model;

namespace Symphony.Configuration
{
    public class LegacySymphonySettings
    {
        private static readonly Encoding Encoding = new UTF8Encoding();
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(WindowPlacement));

        public HotKey HotKeys { get; private set; }
        public bool IsMinimiseOnCloseChecked { get; private set; }
        public WindowPlacement WindowPlacement { get; private set; }

        public void Load()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            path = Path.Combine(path, "Symphony");
            path = Path.Combine(path, "settings.config");

            using (var reader = new SymphonySettingReader(path))
            {
                reader.Load();

                this.IsMinimiseOnCloseChecked = reader.GetValueOrDefault("settings", "minimizeOnClose", false);

                this.HotKeys = new HotKey();
                this.HotKeys.Name = "Bring To Focus";
                this.HotKeys.IsEnabled = reader.GetValueOrDefault("hotkeys", "bringToFocus.isEnabled", false);
                this.HotKeys.ModifierKeys = reader.GetEnumValueOrDefault("hotkeys", "bringToFocus.modifiers", ModifierKeys.Control | ModifierKeys.Shift);
                this.HotKeys.Keys = reader.ConvertEnumValueOrDefault("hotkeys", "bringToFocus.key", Keys.S);

                string placementXml = reader.GetValueOrDefault("window_placement", "xml", string.Empty);

                byte[] xmlBytes = Encoding.GetBytes(placementXml);

                WindowPlacement placement;
                using (var memStream = new MemoryStream(xmlBytes))
                {
                    placement = (WindowPlacement)Serializer.Deserialize(memStream);
                }

                placement.length = Marshal.SizeOf(typeof(WindowPlacement));
                placement.flags = 0;

                this.WindowPlacement = placement;
            }
        }
    }
}
