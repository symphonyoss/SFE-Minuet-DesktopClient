using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using HWND = System.IntPtr;

namespace Symphony.Plugins.MediaStreamPicker
{
    public class EnumWindowResult
    {
        public EnumWindowResult(HWND _hWnd, String _title, BitmapSource _image)
        {
            hWnd = _hWnd;
            title = _title;
            image = _image;
        }
        public HWND hWnd { get; private set; }
        public string title { get; private set; }
        public BitmapSource image { get; private set; }
    }

    public static class EnumerateWindows
    {
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        const UInt32 SW_SHOWMINIMIZED = 2;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(HWND hWnd, ref WINDOWPLACEMENT lpwndpl);

        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PrintWindow(HWND hwnd, IntPtr hDC, uint nFlags);

        // http://msdn.microsoft.com/en-us/library/dd183539(VS.85).aspx
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        // Important note for Vista / Win7 on this function. In those version, rectangle returned is not 100% correct
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HWND hWnd, out Win32Rect rect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(HWND hWnd, out Win32Rect rect);

        // returns all non-minimized windows
        public static IList<EnumWindowResult> getWindows()
        {
            HWND shellWindow = GetShellWindow();

            IList<EnumWindowResult> windows = new List<EnumWindowResult>();
            
            EnumWindows(delegate(HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) 
                    return true;
                
                if (!IsWindowVisible(hWnd)) 
                    return true;

                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                placement.length = Marshal.SizeOf(placement);
                if (!GetWindowPlacement(hWnd, ref placement)) 
                    return true;

                // skip if window is minimized
                if (placement.showCmd == SW_SHOWMINIMIZED) 
                    return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) 
                    return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);
                String title = builder.ToString();

                BitmapSource img = getScreenShot(hWnd);
                if (img == null)
                    return true;

                windows.Add(new EnumWindowResult(hWnd, title, img));

                return true;
            }, 0);

            return windows;
        }


        private static BitmapSource getScreenShot(HWND hwnd)
        {
            bool success = false;
            IntPtr dc = IntPtr.Zero;
            System.Drawing.Graphics memoryGraphics = null;
            System.Drawing.Bitmap bmp = null;
            
            try
            {
                Int32Rect rect = GetWindowActualRect(hwnd);
                bmp = new System.Drawing.Bitmap(rect.Width, rect.Height);
                memoryGraphics = System.Drawing.Graphics.FromImage(bmp);
                dc = memoryGraphics.GetHdc();
                success = PrintWindow(hwnd, dc, 0);
            }
            catch
            {
                return null;
            }
            finally 
            {
                if (dc != null && memoryGraphics != null)
                  memoryGraphics.ReleaseHdc(dc);
            }
            
            if (success == true)
            {
                try
                {
                    return ToBitmapSource(bmp);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            IntPtr ip = source.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                    IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Win32Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;


            public int Width
            {
                get { return Right - Left; }
            }

            public int Height
            {
                get { return Bottom - Top; }
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        struct Win32Point
        {
            public int X;
            public int Y;

            public Win32Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

        }

        // this accounts for the border and shadow. Serious fudgery here.
        static Int32Rect GetWindowActualRect(IntPtr hWnd)
        {
            Win32Rect windowRect = new Win32Rect();
            Win32Rect clientRect = new Win32Rect();

            GetWindowRect(hWnd, out windowRect);
            GetClientRect(hWnd, out clientRect);

            int sideBorder = (windowRect.Width - clientRect.Width) / 2 + 1;

            // sooo, yeah.
            const int hackToAccountForShadow = 4;

            Win32Point topLeftPoint = new Win32Point(windowRect.Left - sideBorder, windowRect.Top - sideBorder);

            //User32.ClientToScreen(hWnd, ref topLeftPoint);

            Int32Rect actualRect = new Int32Rect(
                topLeftPoint.X,
                topLeftPoint.Y,
                windowRect.Width + sideBorder * 2 + hackToAccountForShadow,
                windowRect.Height + sideBorder * 2 + hackToAccountForShadow);

            return actualRect;
        }

        //public static BitmapSource getScreenShot3(HWND hwnd)
        //{
        //    IntPtr hdcScreen = GetDC(IntPtr.Zero);
        //    IntPtr hdcWindow = GetDC(hwnd);

        //    IntPtr hdcMemDC = CreateCompatibleDC(hdcWindow);

        //    Int32Rect rect = GetWindowActualRect(hwnd);
        //    IntPtr hbmScreen = CreateCompatibleBitmap(hdcWindow, rect.Width, rect.Height);
        //    SelectObject(hdcMemDC, hbmScreen);

        //    BitBlt(hdcMemDC, 0, 0, rect.Width, rect.Height, hdcWindow, 0, 0, SRCCOPY);

        //    BitmapSource bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //           hbmScreen, IntPtr.Zero, Int32Rect.Empty,
        //           BitmapSizeOptions.FromEmptyOptions());

        //    DeleteObject(hbmScreen);
        //    ReleaseDC(IntPtr.Zero, hdcWindow);
        //    ReleaseDC(IntPtr.Zero, hdcScreen);

        //    return bitmap;
        //}

        //[DllImportAttribute("user32.dll")]
        //public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        //public static BitmapSource getScreenShot2(HWND hwnd)
        //{
        //    //SetForegroundWindow(hwnd);

        //    return CaptureWindow(hwnd, true, Colors.Transparent, true);
        //}

        // capture a window. This doesn't do the alt-prtscrn version that loses the window shadow.
        // this version captures the shadow and optionally inserts a blank (usually white) area behind
        // it to keep the screen shot clean
        //public static BitmapSource CaptureWindow(IntPtr hWnd, bool recolorBackground, Color substituteBackgroundColor, bool addToClipboard)
        //{
        //    Int32Rect rect = GetWindowActualRect(hWnd);

        //    Window blankingWindow = null;

        //    if (recolorBackground)
        //    {
        //        blankingWindow = new Window();

        //        blankingWindow.WindowStyle = WindowStyle.None;
        //        blankingWindow.Title = string.Empty;
        //        blankingWindow.ShowInTaskbar = false;
        //        blankingWindow.AllowsTransparency = true;
        //        blankingWindow.Background = new SolidColorBrush(substituteBackgroundColor);
        //        blankingWindow.Show();

        //        int fudge = 20;

        //        blankingWindow.Left = rect.X - fudge / 2;
        //        blankingWindow.Top = rect.Y - fudge / 2;
        //        blankingWindow.Width = rect.Width + fudge;
        //        blankingWindow.Height = rect.Height + fudge;

        //    }

        //    // bring the to-be-captured window to capture to the foreground
        //    // there's a race condition here where the blanking window
        //    // sometimes comes to the top. Hate those. There is surely
        //    // a non-WPF native solution to the blanking window which likely
        //    // involves drawing directly on the desktop or the target window

        //    SetForegroundWindow(hWnd);

        //    BitmapSource captured = CaptureRegion(
        //        hWnd,
        //        rect.X,
        //        rect.Y,
        //        rect.Width,
        //        rect.Height,
        //        true);

        //    if (blankingWindow != null)
        //        blankingWindow.Close();

        //    return captured;
        //}

        //// http://msdn.microsoft.com/en-us/library/dd144871(VS.85).aspx
        //[DllImport("user32.dll")]
        //public static extern IntPtr GetDC(IntPtr hwnd);

        //[DllImport("user32.dll")]
        //public static extern IntPtr GetDesktopWindow();

        //// http://msdn.microsoft.com/en-us/library/dd183488(VS.85).aspx
        //[DllImport("gdi32.dll")]
        //public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        //// http://msdn.microsoft.com/en-us/library/dd183489(VS.85).aspx
        //[DllImport("gdi32.dll", SetLastError = true)]
        //public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        //// http://msdn.microsoft.com/en-us/library/dd183370(VS.85).aspx
        //[DllImport("gdi32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, Int32 dwRop);

        //// http://msdn.microsoft.com/en-us/library/dd162957(VS.85).aspx
        //[DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        //public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        //// http://msdn.microsoft.com/en-us/library/dd183539(VS.85).aspx
        //[DllImport("gdi32.dll")]
        //public static extern bool DeleteObject(IntPtr hObject);

        //// http://msdn.microsoft.com/en-us/library/dd162920(VS.85).aspx
        //[DllImport("user32.dll")]
        //public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

        //class ScreenCaptureException : Exception
        //{
        //    public ScreenCaptureException(string message, Exception innerException)
        //        : base(message, innerException)
        //    { }

        //    public ScreenCaptureException(string message)
        //        : base(message)
        //    { }

        //}

        //public const int SRCCOPY = 0xCC0020;

        //// capture a region of a the screen, defined by the hWnd
        //public static BitmapSource CaptureRegion(
        //    IntPtr hWnd, int x, int y, int width, int height, bool addToClipboard)
        //{
        //    IntPtr sourceDC = IntPtr.Zero;
        //    IntPtr targetDC = IntPtr.Zero;
        //    IntPtr compatibleBitmapHandle = IntPtr.Zero;
        //    BitmapSource bitmap = null;

        //    try
        //    {
        //        // gets the main desktop and all open windows
        //        sourceDC = GetDC(GetDesktopWindow());
        //        //sourceDC = User32.GetDC(hWnd);
        //        targetDC = CreateCompatibleDC(sourceDC);

        //        // create a bitmap compatible with our target DC
        //        compatibleBitmapHandle = CreateCompatibleBitmap(sourceDC, width, height);

        //        // gets the bitmap into the target device context
        //        SelectObject(targetDC, compatibleBitmapHandle);

        //        // copy from source to destination
        //        BitBlt(targetDC, 0, 0, width, height, sourceDC, x, y, SRCCOPY);

        //        // Here's the WPF glue to make it all work. It converts from an 
        //        // hBitmap to a BitmapSource. Love the WPF interop functions
        //        bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //            compatibleBitmapHandle, IntPtr.Zero, Int32Rect.Empty,
        //            BitmapSizeOptions.FromEmptyOptions());

        //        if (addToClipboard)
        //        {
        //            //Clipboard.SetImage(bitmap); // high memory usage for large images
        //            IDataObject data = new DataObject();
        //            data.SetData(DataFormats.Dib, bitmap, false);
        //            Clipboard.SetDataObject(data, false);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ScreenCaptureException(string.Format("Error capturing region {0},{1},{2},{3}", x, y, width, height), ex);
        //    }
        //    finally
        //    {
        //        DeleteObject(compatibleBitmapHandle);

        //        ReleaseDC(IntPtr.Zero, sourceDC);
        //        ReleaseDC(IntPtr.Zero, targetDC);
        //    }

        //    return bitmap;
        //}

    }
}
