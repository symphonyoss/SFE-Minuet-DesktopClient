using System;
using Paragon.Plugins;
using Symphony.Mvvm;

namespace Symphony.Shell.SystemMenu.MenuItems
{
    public class ForceClose : SystemMenuItem
    {
        private readonly IApplicationWindow applicationWindow;

        public event EventHandler BeforeClose;

        public ForceClose(IApplicationWindow applicationWindow)
        {
            this.applicationWindow = applicationWindow;

            this.Id = 103;
            this.Header = "Force Close";
            this.Command = new DelegateCommand(this.OnClose);
        }

        private void OnClose()
        {
            var onBeforeClose = this.BeforeClose;
            if (onBeforeClose != null) onBeforeClose(this, EventArgs.Empty);

            this.applicationWindow.CloseWindow();
        }
    }
}