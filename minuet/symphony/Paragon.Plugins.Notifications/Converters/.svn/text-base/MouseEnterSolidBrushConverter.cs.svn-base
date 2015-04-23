using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Paragon.Plugins.Notifications.Converters
{
    public class MouseEnterSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = (SolidColorBrush) value;
            var color = brush.Color;
            var overColor = Color.FromArgb(255, color.R, color.G, color.B);

            return new SolidColorBrush(overColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}