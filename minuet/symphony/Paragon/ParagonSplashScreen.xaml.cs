using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Paragon.Runtime.Win32;
using Paragon.Runtime.WPF;
using Paragon.Runtime;
using Paragon.Plugins;

namespace Paragon
{
    /// <summary>
    /// Interaction logic for ParagonSplashScreen.xaml
    /// </summary>
    public partial class ParagonSplashScreen : INotifyPropertyChanged
    {
        private Storyboard _showboard;
        private Storyboard _hideboard;
        ResourceDictionary _styleRd;
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();

        public ParagonSplashScreen(string shellName, string version, Stream shellIconStream, Stream styleStream)
        {
            InitializeComponent();
            ShellName = shellName;
            Version = version;
            Text = "Initializing...";
            DataContext = this;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.FullPrimaryScreenHeight;
            Left = (screenWidth / 2) - (Width / 2);
            Top = (screenHeight / 2) - (Height / 2);

            if (shellIconStream != null)
            {
                var icon = new Icon(shellIconStream, 128, 128);

                using (var bmp = icon.ToBitmap())
                {
                    var hbmp = bmp.GetHbitmap();

                    try
                    {
                        ShellIcon = Imaging.CreateBitmapSourceFromHBitmap(hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(fmt => fmt("Error creating splash screen bitmap: " + ex));
                    }
                    finally
                    {
                        if (IntPtr.Zero != hbmp)
                            Win32Api.DeleteObject(hbmp);
                    }
                }
            }

            if (ShellIcon == null)
                ShellIcon = new IconImage(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "window.ico")).ImageSource;

            if (styleStream != null)
            {
                try
                {
                    System.Windows.Markup.XamlReader reader = new System.Windows.Markup.XamlReader();
                    _styleRd = (ResourceDictionary)reader.LoadAsync(styleStream);
                }
                catch (Exception ex)
                {
                    Logger.Error(fmt => fmt("Error reading splash screen xaml: " + ex));
                }
                
            }
        }

        public override void OnApplyTemplate()
        {
            if (this.Style == null)
            {
                if (_styleRd != null)
                {
                    if (_styleRd.Contains("Custom"))
                        this.Style = _styleRd["Custom"] as Style;
                }
                else
                    this.Style = Application.Current.Resources["Default"] as Style;
            }

            base.OnApplyTemplate();
            _showboard = Resources["ShowStoryBoard"] as Storyboard;
            _hideboard = Resources["HideStoryBoard"] as Storyboard;
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
            if (_showboard != null)
                BeginStoryboard(_showboard);

            OnPropertyChanged("Text");
            OnPropertyChanged("PreviousText");
            if (_hideboard != null)
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