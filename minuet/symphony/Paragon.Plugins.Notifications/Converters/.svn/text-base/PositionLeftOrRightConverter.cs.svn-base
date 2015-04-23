using System;
using System.Globalization;
using System.Windows.Data;

namespace Paragon.Plugins.Notifications.Converters
{
    public class PositionLeftOrRightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = (Position) value;

            switch (position)
            {
                case Position.BottomLeft:
                case Position.TopLeft:
                    return "Left";
                case Position.BottomRight:
                case Position.TopRight:
                    return "Right";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}