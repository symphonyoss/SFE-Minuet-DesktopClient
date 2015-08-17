using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows;

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    public static class Win32Api
    {
        public delegate void EnumChildProcsDelegate(int parentPid, int childPid);

        public static bool DeleteObject(IntPtr ptr)
        {
            return NativeMethods.DeleteObject(ptr);
        }

        public static void DragMove(IntPtr hwnd)
        {
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(hwnd, WM.SYSCOMMAND, (IntPtr) 0xf012, IntPtr.Zero);
            NativeMethods.SendMessage(hwnd, WM.LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void FlashWindow(IntPtr hwnd, bool clear, bool autoclear = false, int maxFlashes = 5, int timeOut = 0)
        {
            var flags = clear ? FlashWindowFlags.FLASHW_STOP : FlashWindowFlags.FLASHW_ALL;
            flags |= autoclear ? FlashWindowFlags.FLASHW_TIMERNOFG : FlashWindowFlags.FLASHW_TIMER;

            uint flashCount;
            if (maxFlashes < 0)
                flashCount = UInt32.MaxValue;
            else
                flashCount = (uint)maxFlashes;

            var info = new FLASHWINFO
            {
                hwnd = hwnd,
                dwFlags = (uint) flags,
                uCount = (uint) flashCount,
                dwTimeout = (uint) timeOut
            };

            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            NativeMethods.FlashWindowEx(ref info);
        }

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            NativeMethods.GetCursorPos(out lpPoint);
            return lpPoint;
        }

        public static ResizeDirection GetResizeDirection(IntPtr ptr)
        {
            switch ((WMSZ) ptr)
            {
                case WMSZ.TOPLEFT:
                    return ResizeDirection.Top | ResizeDirection.Left;
                case WMSZ.TOP:
                    return ResizeDirection.Top;
                case WMSZ.TOPRIGHT:
                    return ResizeDirection.Top | ResizeDirection.Right;
                case WMSZ.RIGHT:
                    return ResizeDirection.Right;
                case WMSZ.BOTTOMRIGHT:
                    return ResizeDirection.Bottom | ResizeDirection.Right;
                case WMSZ.BOTTOM:
                    return ResizeDirection.Bottom;
                case WMSZ.BOTTOMLEFT:
                    return ResizeDirection.Bottom | ResizeDirection.Left;
                case WMSZ.LEFT:
                    return ResizeDirection.Left;
                default:
                    return ResizeDirection.None;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static int GetParentProcessId(int pid)
        {
            var pbi = new PROCESS_BASIC_INFORMATION();
            using (var procHandle = SafeProcessHandle.OpenProcess(pid, ProcessAccessFlags.QueryInformation))
            {
                int sizeInfo;
                if (NativeMethods.NtQueryInformationProcess(procHandle.DangerousGetHandle(), 0, ref pbi, pbi.Size, out sizeInfo) == 0)
                {
                    return pbi.InheritedFromUniqueProcessId.ToInt32();
                }
            }

            return 0;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void EnumChildProcs(EnumChildProcsDelegate callback, params int[] parentPids)
        {
            if (parentPids.Length == 0)
            {
                return;
            }

            var parentPidList = new List<int>(parentPids);
            parentPidList.Sort();

            var snapshotHandle = IntPtr.Zero;

            try
            {
                var procEntry = new PROCESSENTRY32 {dwSize = (UInt32) Marshal.SizeOf(typeof (PROCESSENTRY32))};
                snapshotHandle = NativeMethods.CreateToolhelp32Snapshot((uint) SnapshotFlags.Process, 0);
                if (!NativeMethods.Process32First(snapshotHandle, ref procEntry))
                {
                    return;
                }

                do
                {
                    var childPid = (int) procEntry.th32ProcessID;
                    var parentPid = GetParentProcessId(childPid);
                    if (parentPid == 0
                        || !parentPidList.Contains(parentPid))
                    {
                        continue;
                    }

                    callback(parentPid, childPid);
                } while (NativeMethods.Process32Next(snapshotHandle, ref procEntry));
            }
            finally
            {
                NativeMethods.CloseHandle(snapshotHandle);
            }
        }

        public static IEnumerable<IntPtr> GetProcessWindows(params int[] pids)
        {
            var result = new List<IntPtr>();

            var callback = new EnumThreadDelegate((hwnd, lParam) =>
            {
                uint processId;
                NativeMethods.GetWindowThreadProcessId(hwnd, out processId);
                if (pids.Contains((int) processId))
                {
                    result.Add(hwnd);
                }

                return true;
            });

            NativeMethods.EnumChildWindows(IntPtr.Zero, callback, IntPtr.Zero);
            return result;
        }

        public static uint GetWindowProcessId(IntPtr hwnd)
        {
            uint processId;
            NativeMethods.GetWindowThreadProcessId(hwnd, out processId);
            return processId;
        }

        public static uint GetWindowThreadId(IntPtr hwnd)
        {
            uint processId;
            return NativeMethods.GetWindowThreadProcessId(hwnd, out processId);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static WindowVisibility GetWindowVisibility(IntPtr hwnd)
        {
            var placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            NativeMethods.GetWindowPlacement(hwnd, ref placement);
            return placement.showCmd;
        }

        public static IEnumerable<IntPtr> GetWindowsByClass(string classname)
        {
            var results = new List<IntPtr>();
            var desktop = NativeMethods.GetDesktopWindow();
            var target = NativeMethods.FindWindowEx(desktop, IntPtr.Zero, classname, null);

            do
            {
                results.Add(target);
                target = NativeMethods.FindWindowEx(desktop, target, classname, null);
            } while (target != IntPtr.Zero);

            return results;
        }

        public static bool MoveWindow(IntPtr hwnd, int x, int y, int width, int height, bool repaint)
        {
            return NativeMethods.MoveWindow(hwnd, x, y, width, height, repaint);
        }

        public static void ActivateWindowNoFocus(IntPtr hWnd)
        {
            SWP flags = SWP.NOACTIVATE | SWP.SHOWWINDOW | SWP.NOSIZE | SWP.NOMOVE;

            NativeMethods.SetWindowPos(hWnd, (IntPtr)(-1), 0, 0, 0, 0, flags);
            NativeMethods.SetWindowPos(hWnd, (IntPtr)(-2), 0, 0, 0, 0, flags);
        }

        public static bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            return NativeMethods.PostMessage(hwnd, msg, wParam, lParam);
        }

        public static IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent)
        {
            return NativeMethods.SetParent(hWndChild, hWndNewParent);
        }

        public static void SetWindowOwner(IntPtr targetHwnd, IntPtr parentHwnd)
        {
            NativeMethods.SetWindowLong(targetHwnd, (int) GWL.HWNDPARENT, (uint) parentHwnd.ToInt32());
        }

        public static void ShowWindow(IntPtr hWnd, SW flags)
        {
            NativeMethods.ShowWindow(hWnd, (int)flags);
        }

        public static void SetWindowPosition(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP flags)
        {
            NativeMethods.SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, flags);
        }

        public static uint GetStyle(IntPtr hWnd)
        {
            return NativeMethods.GetWindowLong(hWnd, (int) GWL.STYLE);
        }

        public static uint GetExStyle(IntPtr hWnd)
        {
            return NativeMethods.GetWindowLong(hWnd, (int) GWL.EXSTYLE);
        }

        public static bool IsAlwaysOnTop(IntPtr hWnd)
        {
            return (GetExStyle(hWnd) & (int) WSEX.TOPMOST) == (int) WSEX.TOPMOST;
        }

        public static void SetAlwaysOnTop(IntPtr hWnd, bool set = true)
        {
            NativeMethods.SetWindowPos(hWnd, set ? new IntPtr(-1) : new IntPtr(-2), 0, 0, 0, 0, (SWP.NOMOVE | SWP.NOSIZE));
        }

        public static bool GetClientRect(IntPtr hWnd, ref RECT rect)
        {
            return NativeMethods.GetClientRect(hWnd, ref rect);
        }

        public static IntPtr GetToplevelParent(IntPtr hWnd)
        {
            return NativeMethods.GetAncestor(hWnd, GAF.ROOT);
        }

        public static IntPtr GetParent(IntPtr hWnd)
        {
            return NativeMethods.GetAncestor(hWnd, GAF.PARENT);
        }

        public static void SetText(IntPtr hWnd, string text)
        {
            NativeMethods.SetWindowText(hWnd, text);
        }

        public static string GetText(IntPtr hWnd)
        {
            var length = NativeMethods.GetWindowTextLength(hWnd);
            var sb = new StringBuilder(length + 1);
            NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static void KillFocus()
        {
            NativeMethods.SetFocus(NativeMethods.GetDesktopWindow());
        }

        public static void SetFocus(IntPtr hWnd)
        {
            NativeMethods.SetFocus(hWnd);
        }

        public static void Maximize(IntPtr hWnd)
        {
            NativeMethods.ShowWindow(hWnd, (int) SWC.Maximize);
        }

        public static bool IsMaximized(IntPtr hWnd)
        {
            var placement = new WINDOWPLACEMENT();
            NativeMethods.GetWindowPlacement(hWnd, ref placement);
            return placement.showCmd == WindowVisibility.Maximized;
        }

        public static void Minimize(IntPtr hWnd)
        {
            NativeMethods.ShowWindow(hWnd, (int) SWC.Minimize);
        }

        public static void Move(IntPtr hWnd, int x, int y)
        {
            NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, SWP.NOSIZE | SWP.NOACTIVATE | SWP.NOZORDER);
        }

        public static void Resize(IntPtr hWnd, int width, int height)
        {
            NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, width, height, SWP.NOSIZE | SWP.NOACTIVATE | SWP.NOZORDER);
        }

        public static bool IsMinimized(IntPtr hWnd)
        {
            var placement = new WINDOWPLACEMENT();
            NativeMethods.GetWindowPlacement(hWnd, ref placement);
            return placement.showCmd == WindowVisibility.Minimized;
        }

        public static void Restore(IntPtr hWnd)
        {
            NativeMethods.ShowWindow(hWnd, (int) SWC.Restore);
        }

        public static void Show(IntPtr hWnd)
        {
            NativeMethods.ShowWindow(hWnd, (int) SWC.Show);
        }

        public static void Hide(IntPtr hWnd)
        {
            NativeMethods.ShowWindow(hWnd, (int) SWC.Hide);
        }

        public static void Close(IntPtr hWnd)
        {
            NativeMethods.SendMessage(hWnd, WM.SYSCOMMAND, new IntPtr(0xF060) /* SC_CLOSE */, IntPtr.Zero);
        }

        public static void Restore(IntPtr handle, ref WINDOWPLACEMENT placement)
        {
            var style = GetStyle(handle);
            if ((style & (int) WS.OVERLAPPEDWINDOW) != (int) WS.OVERLAPPEDWINDOW)
            {
                NativeMethods.SetWindowLong(handle, (int) GWL.STYLE, (style | (int) WS.OVERLAPPEDWINDOW));
                placement.showCmd = WindowVisibility.Normal;
                NativeMethods.SetWindowPlacement(handle, ref placement);
                NativeMethods.SetWindowPos(handle, IntPtr.Zero, placement.rcNormalPosition.Left, placement.rcNormalPosition.Top,
                    placement.rcNormalPosition.Right - placement.rcNormalPosition.Left,
                    placement.rcNormalPosition.Top - placement.rcNormalPosition.Bottom, SWP.NOMOVE | SWP.NOSIZE | SWP.FRAMECHANGED);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static WINDOWPLACEMENT FullScreen(IntPtr hWnd)
        {
            var placement = new WINDOWPLACEMENT();
            var style = GetStyle(hWnd);
            if ((style & (long) WS.OVERLAPPEDWINDOW) == (long) WS.OVERLAPPEDWINDOW)
            {
                uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
                var mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(mi);

                if (NativeMethods.GetWindowPlacement(hWnd, ref placement) &&
                    NativeMethods.GetMonitorInfo(NativeMethods.MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY), ref mi))
                {
                    NativeMethods.SetWindowLong(hWnd, (int) GWL.STYLE, style & (uint) ~WS.OVERLAPPEDWINDOW);
                    NativeMethods.SetWindowPos(hWnd, IntPtr.Zero, mi.rcMonitor.Left, mi.rcMonitor.Top,
                        mi.rcMonitor.Width,
                        mi.rcMonitor.Height,
                        SWP.NOOWNERZORDER | SWP.FRAMECHANGED);
                }
            }
            return placement;
        }

        public static IntPtr GetChildWindow(IntPtr hWnd)
        {
            return NativeMethods.GetWindow(hWnd, 5 /* GW_CHILD */);
        }

        public static IntPtr LoadLibrary(string name)
        {
            return NativeMethods.LoadLibrary(name);
        }
    }
}