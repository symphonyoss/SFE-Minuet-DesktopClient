using Paragon.Plugins.Notifications.Mvvm;

namespace Paragon.Plugins.Notifications.Models
{
    public class NotificationButton : BindableBase
    {
        private string iconUrl;
        private string title;

        public string IconUrl
        {
            get { return iconUrl; }
            set
            {
                if (value == iconUrl)
                {
                    return;
                }
                iconUrl = value;
                OnPropertyChanged("IconUrl");
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (value == title)
                {
                    return;
                }
                title = value;
                OnPropertyChanged("Title");
            }
        }
    }
}