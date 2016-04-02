using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HWND = System.IntPtr;

namespace Symphony.Plugins.MediaStreamPicker
{
    public class Img
    {
        public Img(string value, BitmapSource img) 
        { 
            Str = value; 
            ImageSource = img; 
        }
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

        IList<EnumScreenResult> screens = null;
        IList<EnumWindowResult> windows = null;

        HWND _myHwnd;

        System.Threading.Timer _timer;
        object _locker = new object(); // lock for thread getting updates

        public MediaStreamPicker()
        {
            InitializeComponent();

            this.Loaded += MediaStreamPicker_Loaded;
            this.Unloaded += MediaStreamPicker_Unloaded;
            share.Click += onClickedShare;
            cancel.Click += onClickedCancel;
        }

        void MediaStreamPicker_Loaded(object sender, RoutedEventArgs e)
        {
            _myHwnd = getMyHwnd();

            // runs on a seperate thread, because enumeration is expensive and interferes with ui thread.
            _timer = new System.Threading.Timer(_onTimer, null, new TimeSpan(0), new TimeSpan(0, 0, 3));
        }

        HWND getMyHwnd()
        {
            Window window = Window.GetWindow(this);
            var wih = new WindowInteropHelper(window);
            return wih.Handle;
        }

        void MediaStreamPicker_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Dispose();
        }

        void onClickedShare(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void onClickedCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void _onTimer(object state)
        {
            if (System.Threading.Monitor.TryEnter(_locker))
            {
                try
                {
                    IList<EnumScreenResult> newScreens = EnumerateScreens.getScreens();
                    IList<EnumWindowResult> newWindows = EnumerateWindows.getWindows(_myHwnd);

                    bool rebuild = false;

                    if (screens == null || windows == null ||
                        newWindows.Count != windows.Count || newScreens.Count != screens.Count)
                    {
                        rebuild = true;
                    }
                    else
                    {
                        foreach (EnumWindowResult window in windows)
                        {
                            bool found = false;
                            foreach (EnumWindowResult newWindow in newWindows)
                            {
                                if (newWindow.hWnd == window.hWnd && newWindow.title == window.title)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                rebuild = true;
                                break;
                            }
                        }
                    }

                    if (!rebuild)
                        return;

                    screens = newScreens;
                    windows = newWindows;

                    // signal to main thread to rebuild
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        this.rebuild();
                    }));
                }
                finally
                {
                    System.Threading.Monitor.Exit(_locker);
                }
            }
        }

        void rebuild()
        {
            streams.Items.Clear();
            mediaStreams.Clear();

            foreach (EnumScreenResult screen in screens)
            {
                addToStreams(screen.title, screen.image);
                mediaStreams.Add("screen:" + screen.id);
            }
            foreach (EnumWindowResult window in windows)
            {
                addToStreams(window.title, window.image);
                mediaStreams.Add("window:" + window.hWnd.ToString());
            }

            streams.SelectedIndex = -1;
        }

        void addToStreams(string title, BitmapSource image)
        {
            Img item = new Img(title, image);
            streams.Items.Add(item);
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
