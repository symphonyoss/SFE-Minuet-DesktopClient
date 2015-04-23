using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Forms;

namespace Symphony.Plugins.Hotkeys.Converters
{
    public class KeysValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            if (!string.IsNullOrEmpty(str))
            {
                return (Keys)Enum.Parse(typeof(Keys), str, true);
            }

            return string.Empty;
        }
    }
}
