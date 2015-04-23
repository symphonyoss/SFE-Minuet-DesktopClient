using System.Windows;
using System.Windows.Input;

namespace Symphony.Shell.SystemMenu
{
    public class SystemMenuItem : Freezable
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof (ICommand), typeof (SystemMenuItem), new PropertyMetadata(OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof (object), typeof (SystemMenuItem), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof (string), typeof (SystemMenuItem), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof (int), typeof (SystemMenuItem), new PropertyMetadata(default(int)));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public string Header
        {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SystemMenuItem();
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemMenuItem systemMenuItem = d as SystemMenuItem;

            if (systemMenuItem != null)
            {
                if (e.NewValue != null)
                {
                    systemMenuItem.Command = e.NewValue as ICommand;
                }
            }
        }
    }
}
