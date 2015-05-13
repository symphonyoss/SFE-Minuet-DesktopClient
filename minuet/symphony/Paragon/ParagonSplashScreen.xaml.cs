using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Paragon.Runtime.WPF;

namespace Paragon
{
    /// <summary>
    /// Interaction logic for ParagonSplashScreen.xaml
    /// </summary>
    public partial class ParagonSplashScreen : INotifyPropertyChanged
    {
        private readonly Storyboard _showboard;
        private readonly Storyboard _hideboard;

        public ParagonSplashScreen(string shellName, ImageSource shellIcon, string version)
        {
            InitializeComponent();
            ShellName = shellName;
            ShellIcon = shellIcon;
            Version = version;
            Text = "Initializing...";
            DataContext = this;
            _showboard = Resources["ShowStoryBoard"] as Storyboard;
            _hideboard = Resources["HideStoryBoard"] as Storyboard;

            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.FullPrimaryScreenHeight;
            Left = (screenWidth / 2) - (Width / 2);
            Top = (screenHeight / 2) - (Height / 2);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ShellName { get; set; }

        public ImageSource ShellIcon { get; private set; }

        public string Version { get; set; }

        public string Text { get; private set; }

        public string PreviousText { get; private set; }

        public void ShowText(string text)
        {
            PreviousText = Text;
            Text = text;
            BeginStoryboard(_showboard);
            OnPropertyChanged("Text");
            OnPropertyChanged("PreviousText");
            BeginStoryboard(_hideboard);
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
