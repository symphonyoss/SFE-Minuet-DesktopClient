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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HWND = System.IntPtr;

namespace Symphony.Plugins.MediaStreamPicker
{
    public class Img
    {
        public Img(string value, Image img) { Str = value; Image = img; }
        public string Str { get; set; }
        public Image Image { get; set; }
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

            done.Click += onClickedDone;
        }

        void onClickedDone(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void MediaStreamPicker_Loaded(object sender, RoutedEventArgs e)
        {
            IDictionary<HWND, EnumWindowsResult> result = EnumerateWindows.GetOpenWindows();

            //HWND ptrZero = new HWND(0);
            //result.Add(ptrZero, "Desktop");

            var enumerator = result.GetEnumerator();

            while (enumerator.MoveNext())
            {
                EnumWindowsResult value = enumerator.Current.Value;

                Image i = new Image();
                i.Width = 200;
                i.Height = 200;
                i.Source = value.Image;

                Img item = new Img(value.Title, i);
                streams.Items.Add(item);
                string streamType = "window";
                //if (value == "Desktop")
                //    streamType = "screen";

                mediaStreams.Add(streamType + ":" + enumerator.Current.Key.ToString());
            }

            streams.SelectedIndex = streams.Items.Count - 1;
        }

        void onSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            ListBox lb = sender as ListBox;
            ListBoxItem lbi = (lb.SelectedItem as ListBoxItem);
            int index = lb.SelectedIndex;
            string mediaStream = mediaStreams[index];
            selectedMediaStream = mediaStream;
            selectedHwndLabel.Content = mediaStream;
        }

        public string getSelectedMediaStream()
        {
            return selectedMediaStream;
        }
    }
}
