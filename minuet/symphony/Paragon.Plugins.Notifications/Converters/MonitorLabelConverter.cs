using System;
using System.Globalization;
using System.Windows.Data;

namespace Paragon.Plugins.Notifications.Converters
{
    public class MonitorLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var monitorIndex = (int) value;
            return monitorIndex + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}