using Paragon.Plugins;
using Symphony.Mvvm;

namespace Symphony.Shell.SystemMenu.MenuItems
{
    public class Refresh : SystemMenuItem
    {
        private readonly IApplicationWindow applicationWindow;

        public Refresh(IApplicationWindow applicationWindow)
        {
            this.applicationWindow = applicationWindow;

            this.Id = 100;
            this.Header = "Refresh";
            this.Command = new DelegateCommand(this.OnRefresh);
        }

        private void OnRefresh()
        {
            //this.applicationWindow.Refresh();
        }
    }
}
