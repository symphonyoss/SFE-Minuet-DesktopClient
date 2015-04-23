using System.Windows;
using System.Windows.Controls;

namespace Paragon.Runtime.WPF
{
    public class WindowCommands : ItemsControl
    {
        static WindowCommands()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (WindowCommands), new FrameworkPropertyMetadata(typeof (WindowCommands)));
        }
    }
}