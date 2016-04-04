using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using HWND = System.IntPtr;

namespace Symphony.Plugins.MediaStreamPicker
{
    public class RequestShareEventArgs: EventArgs
    {
        public RequestShareEventArgs(string mediaStream)
        {
            this.mediaStream = mediaStream;
        }

        public string mediaStream { get; private set; }
    };

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


    public class MediaStreamPickerViewModel : INotifyPropertyChanged
    {
        Window _viewWindow;
        private HWND _viewHwnd;

        List<string> mediaStreams = new List<string>();

        IList<EnumScreenResult> screens = null;
        IList<EnumWindowResult> windows = null;

        System.Threading.Timer _timer;
        object _locker = new object(); // lock for thread getting updates

        ObservableCollection<Img> streams = new ObservableCollection<Img>();
        int selectedIndex = -1;
        bool isShareEnabled = false;

        public MediaStreamPickerViewModel(Window window)
        {
            _viewWindow = window;

            this.CancelCommand = new CommandHandler(this.OnCancel);
            this.ShareCommand = new CommandHandler(this.OnShare);

            window.Loaded += window_Loaded;
            window.Unloaded += window_Unloaded;
        }

        void window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewHwnd = getWindowHwnd(_viewWindow); 
            // runs on a seperate thread, because enumeration is expensive and interferes with ui thread.
            _timer = new System.Threading.Timer(_onTimer, null, new TimeSpan(0), new TimeSpan(0, 0, 3));
        }

        void window_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Dispose();
        }

        HWND getWindowHwnd(Window window)
        {
            Window win = Window.GetWindow(window);
            var wih = new WindowInteropHelper(win);
            return wih.Handle;
        }

        void _onTimer(object state)
        {
            if (System.Threading.Monitor.TryEnter(_locker))
            {
                try
                {
                    IList<EnumScreenResult> newScreens = EnumerateScreens.getScreens();
                    IList<EnumWindowResult> newWindows = EnumerateWindows.getWindows(_viewHwnd);

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
            streams.Clear();
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

            Streams = streams;
            selectedIndex = -1;
        }

        void addToStreams(string title, BitmapSource image)
        {
            Img item = new Img(title, image);
            streams.Add(item);
        }

        string getSelectedMediaStream()
        {
            if (selectedIndex == -1 || selectedIndex < 0 || selectedIndex >= mediaStreams.Count)
                return null;

            return mediaStreams[selectedIndex];
        }

        public ObservableCollection<Img> Streams
        {
            get { return this.streams; }
            set
            {
                this.streams = value;
                this.OnPropertyChanged("Streams");
            }
        }

        public int SelectedIndex
        {
            get { return this.selectedIndex;  }
            set
            {
                if (value == this.selectedIndex) return;
                this.selectedIndex = value;
                this.OnPropertyChanged("SelectedIndex");
                IsShareEnabled = this.selectedIndex != -1 ? true : false;
            }
        }

        public bool IsShareEnabled
        {
            get { return this.isShareEnabled; }
            set
            {
                if (value == this.isShareEnabled) return;
                this.isShareEnabled = value;
                this.OnPropertyChanged("IsShareEnabled");
            }
        }

        public event EventHandler<RequestShareEventArgs> RequestShare;
        public event EventHandler RequestCancel;

        public ICommand ShareCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        protected virtual void OnShare()
        {
            string selectedMediaStream = getSelectedMediaStream();
            var onRequestShare = this.RequestShare;
            if (onRequestShare != null) onRequestShare(this, new RequestShareEventArgs(selectedMediaStream));
        }

        protected virtual void OnCancel()
        {
            var onCancel = this.RequestCancel;
            if (onCancel != null) onCancel(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private class CommandHandler : ICommand
        {
            private Action _action;
            public CommandHandler(Action action)
            {
                this._action = action;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                this._action();
            }
        }
    }
}
