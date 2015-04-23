using System;
using System.Reflection;
using System.Windows.Input;

namespace Symphony.Shell.SystemMenu.MenuItems
{
    public class SymphonyVersion : SystemMenuItem
    {
        public SymphonyVersion()
        {
            this.Id = 110;
            this.Header = Assembly.GetCallingAssembly().GetName().Version.ToString();
            this.Command = new DisableCommand();
        }

        private class DisableCommand : ICommand
        {
            public bool CanExecute(object parameter)
            {
                return false;
            }

            public void Execute(object parameter)
            {

            }

            public event EventHandler CanExecuteChanged;
        }
    }
}