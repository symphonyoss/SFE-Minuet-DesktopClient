using System;
using System.Threading;
using System.Windows.Input;

namespace Symphony.Mvvm
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
            add
            {
                this.CanExecuteChanged += value;
            }
            remove
            {
                this.CanExecuteChanged -= value;
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            this.executeMethod();
        }

        protected void Execute(object parameter)
        {
            this.executeMethod();
        }

        private event EventHandler CanExecuteChanged
        {
            add
            {
                EventHandler eventHandler = this.canExecuteChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref this.canExecuteChanged, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler eventHandler = this.canExecuteChanged;
                EventHandler comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange(ref this.canExecuteChanged, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }
    }
}