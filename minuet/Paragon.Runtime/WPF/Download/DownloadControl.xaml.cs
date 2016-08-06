using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wpf.Controls;

namespace Paragon.Runtime.WPF.Download
{
    /// <summary>
    /// Interaction logic for DownloadControl.xaml
    /// </summary>
    public partial class DownloadControl : UserControl
    {
        public DownloadControl()
        {
            InitializeComponent();
            closeButton.Click += new RoutedEventHandler(OnCloseButtonClicked);
        }

        class DownloadItem : SplitButton
        {
            uint _id; // unique id

            TextBlock _nameText;
            TextBlock _sizeText;

            String _fileName;
            String _fullPath;
            //int _progress = 0; // percent progress
            long _receivedBytes;
            bool _isComplete = false;
            bool _isCanceled = false;

            MenuItem _openItem;
            MenuItem _showInFolderItem;
            MenuItem _cancelItem;

            public DownloadItem(uint id, String fileName, String fullPath, long receivedBytes, bool isComplete, bool isCanceled)
            {
                _id = id;
                _fileName = fileName;
                _fullPath = fullPath;

                _receivedBytes = receivedBytes;
                //_progress = progress;

                IsEnabled = false;

                this.isComplete = isComplete;
                this.isCanceled = isCanceled;

                // split mode button options
                Mode = SplitButtonMode.Split;
                Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;

                this.Loaded += DownloadItem_Loaded;

                Margin = new Thickness(2);

                StackPanel buttonPanel = new StackPanel();
                buttonPanel.Margin = new Thickness(5, 0, 0, 0);
                buttonPanel.Width = 150;
                buttonPanel.Orientation = Orientation.Vertical;

                _nameText = new TextBlock();
                _nameText.TextTrimming = TextTrimming.CharacterEllipsis;

                _sizeText = new TextBlock();

                updateText();

                buttonPanel.Children.Add(_nameText);
                buttonPanel.Children.Add(_sizeText);

                Content = buttonPanel;

                _openItem = new MenuItem();
                _openItem.Header = "Open";
                _openItem.IsEnabled = isComplete;
                _openItem.Click += openItem_Click;

                _showInFolderItem = new MenuItem();
                _showInFolderItem.Header = "Show In Folder";
                _showInFolderItem.IsEnabled = true;
                _showInFolderItem.Click += showInFolderItem_Click;

                _cancelItem = new MenuItem();
                _cancelItem.Header = "Cancel";
                _cancelItem.IsEnabled = !isComplete;
                _cancelItem.Click += cancelItem_Click;

                this.Items.Add(_openItem);
                this.Items.Add(_showInFolderItem);
                this.Items.Add(_cancelItem);
            }

            bool _shouldCancel = false;
            public bool ShouldCancel
            {
                get { return _shouldCancel; }

            }

            void cancelItem_Click(object sender, RoutedEventArgs e)
            {
                _shouldCancel = true;
            }

            void showInFolderItem_Click(object sender, RoutedEventArgs e)
            {
                if (System.IO.File.Exists(_fullPath))
                {
                    try
                    {
                        Process.Start("explorer.exe", @"/select, " + _fullPath);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("exception: " + exception.ToString());
                        ShowFileNotFoundMessage(_fullPath);
                    }
                }
                else
                    ShowFileNotFoundMessage(_fullPath);
            }

            void ShowFileNotFoundMessage(String fullpath)
            {
                MessageBox.Show("File not Found: " + fullpath, "File not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            void openItem_Click(object sender, RoutedEventArgs e)
            {
                OpenItem();
            }

            void DownloadItem_Loaded(object sender, RoutedEventArgs e)
            {
                // can't get resources until control has been loaded
                Style s = TryFindResource("lunaMetallicSplitButtonStyle") as Style;
                if (s != null)
                    Style = s;
            }

            Process _process;

            DispatcherTimer _enableButtonTimer;


            void DownloadItem_Click(object sender, RoutedEventArgs e)
            {
                OpenItem();
            }

            void OpenItem()
            {
                try
                {
                    if (_process == null)
                    {
                        _process = new Process();
                        _process.StartInfo = new ProcessStartInfo(_fullPath);
                        _process.Exited += _process_Exited;
                    }
                    IsEnabled = false;
                    if (_enableButtonTimer == null)
                    {
                        _enableButtonTimer = new DispatcherTimer();
                        _enableButtonTimer.Tick += _enableButtonTimer_Tick;
                        _enableButtonTimer.Interval = new TimeSpan(0, 0, 3);
                    }
                    _enableButtonTimer.Start();
                    _process.Start();
                }
                catch (Exception exception)
                {
                    Console.WriteLine("exception: " + exception.ToString());
                    MessageBox.Show("Could not open file: " + _fullPath + Environment.NewLine + Environment.NewLine +
                        "File doesn't exist or no installed program can open this file type.",
                        "Open File", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    _process = null;
                }
            }

            void _enableButtonTimer_Tick(object sender, EventArgs e)
            {
                if (_enableButtonTimer == null)
                    return;

                _enableButtonTimer.Stop();
                IsEnabled = true;
            }

            void _process_Exited(object sender, EventArgs e)
            {
                _process.Exited -= _process_Exited;
                _process = null;
            }

            public long ReceivedBytes
            {
                get { return _receivedBytes; }
                set
                {
                    _receivedBytes = value;
                    updateText();
                }
            }

            public bool isCanceled
            {
                get { return _isCanceled; }
                set
                {
                    _isCanceled = value;
                    IsEnabled = !value;
                }
            }

            public bool isComplete
            {
                get { return _isComplete; }
                set
                {
                    _isComplete = value;
                    IsEnabled = value;

                    if (_cancelItem != null)
                        _cancelItem.IsEnabled = !value;

                    if (_openItem != null)
                        _openItem.IsEnabled = value;
                }
            }

            static readonly string[] SizeSuffixes = { "bytes", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };
            static string SizeSuffix(long value)
            {
                if (value < 0) { return "-" + SizeSuffix(-value); }
                if (value == 0) { return "0.0 bytes"; }

                int mag = (int)Math.Log(value, 1024);
                decimal adjustedSize = (decimal)value / (1L << (mag * 10));

                return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
            }

            void updateText()
            {
                _nameText.Text = _fileName;
                if (isCanceled)
                    _sizeText.Text = "download canceled";
                else
                    _sizeText.Text = SizeSuffix(ReceivedBytes);
            }

            public uint id
            {
                get { return _id; }
            }
        }

        public void AddItem(uint id, String FileName, String FullPath, long ReceivedBytes, bool isComplete, bool isCanceled)
        {
            foreach (DownloadItem i in itemsPanel.Children)
            {
                if (i.id == id)
                {
                    return; // already exists
                }
            }

            DownloadItem item = new DownloadItem(id, FileName, FullPath, ReceivedBytes, isComplete, isCanceled);
            itemsPanel.Children.Insert(0, item);
        }

        public bool UpdateItem(uint id, long ReceivedBytes, bool isComplete, bool isCanceled)
        {
            foreach (DownloadItem i in itemsPanel.Children)
            {
                if (i.id == id)
                {
                    i.isComplete = isComplete;
                    i.isCanceled = isCanceled;
                    i.ReceivedBytes = ReceivedBytes;
                    return i.ShouldCancel;
                }
            }

            return false;
        }

        public delegate void CloseHandler(object sender, EventArgs e);
        public event CloseHandler CloseHandlerEvent;

        void closeButtonClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            if (CloseHandlerEvent != null)
                CloseHandlerEvent(this, new EventArgs());
            e.Handled = true;
        }

        public void buttonChrome_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            SplitButton b = (SplitButton)sender;
            b.Button_Click();
            e.Handled = true;
        }

        public void chrome_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            SplitButton b = (SplitButton)sender;
            b.Dropdown_Click();
            e.Handled = true;
        }

    }
}
