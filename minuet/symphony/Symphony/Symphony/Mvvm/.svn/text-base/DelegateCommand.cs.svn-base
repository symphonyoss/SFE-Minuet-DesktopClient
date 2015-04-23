using System;

namespace Symphony.Mvvm
{
    public class DelegateCommand : DelegateCommandBase
    {
        public DelegateCommand(Action executeMethod)
            : base(executeMethod)
        {
        }

        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            base.Execute((object) null);
        }
    }
}
