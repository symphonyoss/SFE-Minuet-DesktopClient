using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HWND = System.IntPtr;

namespace Symphony.Plugins.MediaStreamPicker
{
    public class Img
    {
        public Img(string value, BitmapSource img) { Str = value; ImageSource = img; }
        public string Str { get; set; }
        public BitmapSource ImageSource { get; set; }
    }

    /// <summary>
    /// Interaction logic for MediaStreamPicker.xaml
    /// </summary>
    public partial class MediaStreamPicker : Window
    {
        List<string> mediaStreams = new List<string>();
        string selectedMediaStream = null;

        public MediaStreamPicker()
        {
            InitializeComponent();

            this.Loaded += MediaStreamPicker_Loaded;

            share.Click += onClickedShare;
            cancel.Click += onClickedCancel;

            //addToStreams("test", null);
        }

        void onClickedShare(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void onClickedCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void addToStreams(string title, BitmapSource image)
        {
            Img item = new Img(title, image);
            streams.Items.Add(item);
        }

        void MediaStreamPicker_Loaded(object sender, RoutedEventArgs e)
        {
            IList<EnumScreenResult> screens = EnumerateScreens.getScreens();
            foreach (EnumScreenResult screen in screens)
            {
                addToStreams(screen.title, screen.image);
                mediaStreams.Add("screen:" + screen.id);
            }

            IList<EnumWindowResult> windows = EnumerateWindows.getWindows();
            foreach (EnumWindowResult window in windows)
            {
                addToStreams(window.title, window.image);
                mediaStreams.Add("window:" + window.hWnd.ToString());
            }

            streams.SelectedIndex = -1;
        }

        void onSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListBox lb = sender as ListBox;
            ListBoxItem lbi = (lb.SelectedItem as ListBoxItem);
            int index = lb.SelectedIndex;

            if (index != -1 && index < mediaStreams.Count - 1)
            {
                string mediaStream = mediaStreams[index];
                selectedMediaStream = mediaStream;
            }

            share.IsEnabled = (index != -1) ? true : false;
        }

        public string getSelectedMediaStream()
        {
            return selectedMediaStream;
        }
    }
}
