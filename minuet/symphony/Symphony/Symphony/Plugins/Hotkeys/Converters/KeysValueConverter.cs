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
            try
            {
                if (!(value is Keys))
                {
                    return null;
                }

                var key = (Keys)value;
                if (key >= Keys.D0 && key <= Keys.D9)
                {
                    var val = Enum.GetName(typeof(Keys), key);
                    if (val != null)
                    {
                        return val.Substring(1);
                    }

                    return null;
                }

                switch (key)
                {
                    case Keys.OemOpenBrackets:
                        return "[";
                    case Keys.OemCloseBrackets:
                        return "]";
                    case Keys.OemBackslash:
                        return @"\";
                    case Keys.Oemtilde:
                        return "`";
                    case Keys.OemPeriod:
                        return ".";
                    case Keys.Oemcomma:
                        return ",";
                    case Keys.OemQuestion:
                        return "/";
                    case Keys.OemSemicolon:
                        return ";";
                    case Keys.Oemplus:
                        return "=";
                    case Keys.OemMinus:
                        return "-";
                    default:
                        return Enum.GetName(typeof(Keys), key);
                }
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var keyStr = value as string;
                if (string.IsNullOrEmpty(keyStr))
                {
                    return Keys.None;
                }

                var keyChar = System.Convert.ToChar(keyStr);

                switch (keyChar)
                {
                    case '{':
                    case '[':
                        return Keys.OemOpenBrackets;
                    case '}':
                    case ']':
                        return Keys.OemCloseBrackets;
                    case '|':
                    case '\\':
                        return Keys.OemBackslash;
                    case '`':
                    case '~':
                        return Keys.Oemtilde;
                    case '>':
                    case '.':
                        return Keys.OemPeriod;
                    case '<':
                    case ',':
                        return Keys.Oemcomma;
                    case '/':
                    case '?':
                        return Keys.OemQuestion;
                    case ';':
                        return Keys.OemSemicolon;
                    case '=':
                    case '+':
                        return Keys.Oemplus;
                    case '_':
                    case '-':
                        return Keys.OemMinus;
                    case '"':
                    case '\'':
                        return Keys.OemQuotes;

                    default:
                        int keyVal;
                        if (int.TryParse(keyStr, out keyVal)
                            && (keyVal >= 0 && keyVal <= 9))
                        {
                            return (Keys) Enum.Parse(typeof (Keys), "D" + keyVal, true);
                        }

                        return (Keys)Enum.Parse(typeof(Keys), keyStr, true);
                }
            }
            catch
            {
                return Keys.None;
            }
        }
    }
}
