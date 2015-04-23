using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Paragon.HotKeys.Converters
{
    public class ModifierKeysValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var modifier = (ModifierKeys) value;

            switch (modifier)
            {
                case ModifierKeys.Control:
                    return "Ctrl";

                case ModifierKeys.Alt:
                    return "Alt";

                case ModifierKeys.Control | ModifierKeys.Shift:
                    return "Ctrl+Shift";

                case ModifierKeys.Alt | ModifierKeys.Shift:
                    return "Alt+Shift";

                case ModifierKeys.Control | ModifierKeys.Alt:
                    return "Ctrl+Alt";

                default:
                    throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
