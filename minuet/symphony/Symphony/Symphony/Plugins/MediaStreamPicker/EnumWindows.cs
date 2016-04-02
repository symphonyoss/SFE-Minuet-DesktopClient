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

        const UInt32 SW_HIDE = 0;
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

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        // returns all non-minimized windows
        public static IList<EnumWindowResult> getWindows(HWND currentHwnd)
        {
            HWND shellWindow = GetShellWindow();

            IList<EnumWindowResult> windows = new List<EnumWindowResult>();
            
            EnumWindows(delegate(HWND hWnd, int lParam)
            {
                if (hWnd == currentHwnd || hWnd == shellWindow || 
                    !IsWindowVisible(hWnd) || !isAllowedWindow(hWnd) ||
                    !isWindowPlacementValid(hWnd))
                    return true;

                String title = getWindowTitle(hWnd);
                if (String.IsNullOrEmpty(title)) 
                    return true;

                BitmapSource img = getScreenShot(hWnd);
                if (img == null)
                    return true;

                // freeze needed since we are creating on a separate thread
                img.Freeze();

                windows.Add(new EnumWindowResult(hWnd, title, img));

                return true;
            }, 0);

            return windows;
        }

        static bool isAllowedWindow(HWND hWnd)
        {
            int nRet;
            StringBuilder className = new StringBuilder(256);
            nRet = GetClassName(hWnd, className, className.Capacity);
            if (nRet != 0)
            {
                var name = className.ToString();
                // Skip Program Manager window and the Start button. This is the same logic
                // that's used in Win32WindowPicker in libjingle. Consider filtering other
                // windows as well (e.g. toolbars).
                if (name == "Progman" || name == "Button")
                    return false;
                
                // can't get screen shot of "modern apps", filter them out.
                if (IsWindows8OrGreater() && name == "ApplicationFrameWindow" ||
                    name == "Windows.UI.Core.CoreWindow")
                    return false;
            }

            return true;
        }

        static bool IsWindows8OrGreater()
        {
            // https://msdn.microsoft.com/library/windows/desktop/ms724832.aspx
            return Environment.OSVersion.Version >= new Version("6.2");
        }

        static bool isWindowPlacementValid(HWND hWnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            if (!GetWindowPlacement(hWnd, ref placement))
                return false;

            // skip if window is minimized or hidden
            if (placement.showCmd == SW_SHOWMINIMIZED || placement.showCmd == SW_HIDE)
                return false;

            return true;
        }

        static string getWindowTitle(HWND hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0)
                return null;

            StringBuilder builder = new StringBuilder(length);
            GetWindowText(hWnd, builder, length + 1);
            return builder.ToString();
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

        static Int32Rect GetWindowActualRect(IntPtr hWnd)
        {
            Win32Rect windowRect = new Win32Rect();
            Win32Rect clientRect = new Win32Rect();

            GetWindowRect(hWnd, out windowRect);
            GetClientRect(hWnd, out clientRect);

            int sideBorder = (windowRect.Width - clientRect.Width) / 2 + 1;

            const int hackToAccountForShadow = 4;

            Win32Point topLeftPoint = new Win32Point(windowRect.Left - sideBorder, windowRect.Top - sideBorder);

            Int32Rect actualRect = new Int32Rect(
                topLeftPoint.X,
                topLeftPoint.Y,
                windowRect.Width + sideBorder * 2 + hackToAccountForShadow,
                windowRect.Height + sideBorder * 2 + hackToAccountForShadow);

            return actualRect;
        }
    }
}
