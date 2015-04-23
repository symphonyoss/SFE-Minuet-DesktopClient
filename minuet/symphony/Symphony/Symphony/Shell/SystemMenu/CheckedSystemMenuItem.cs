using System.Windows;

namespace Symphony.Shell.SystemMenu
{
    public class CheckedSystemMenuItem : SystemMenuItem
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(CheckedSystemMenuItem), new PropertyMetadata(default(bool)));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CheckedSystemMenuItem();
        }
    }
}