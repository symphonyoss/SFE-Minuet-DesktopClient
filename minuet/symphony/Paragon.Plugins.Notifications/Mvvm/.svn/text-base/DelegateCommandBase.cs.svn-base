using System;
using System.Threading;
using System.Windows.Input;

namespace Paragon.Plugins.Notifications.Mvvm
{
    public abstract class DelegateCommandBase : ICommand
    {
        private readonly Action executeMethod;
        private EventHandler canExecuteChanged;

        protected DelegateCommandBase(Action executeMethod)
        {
            this.executeMethod = executeMethod;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CanExecuteChanged += value; }
            remove { CanExecuteChanged -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            executeMethod();
        }

        protected void Execute(object parameter)
        {
            executeMethod();
        }

        private event EventHandler CanExecuteChanged
        {
            add
            {
                var eventHandler = canExecuteChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref canExecuteChanged, comparand + value, comparand);
                } while (eventHandler != comparand);
            }
            remove
            {
                var eventHandler = canExecuteChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref canExecuteChanged, comparand - value, comparand);
                } while (eventHandler != comparand);
            }
        }
    }
}