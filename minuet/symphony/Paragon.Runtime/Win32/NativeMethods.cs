//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Paragon.Runtime.Win32
{
    internal delegate bool EnumThreadDelegate(IntPtr hwnd, IntPtr lParam);
    internal delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

    [ExcludeFromCodeCoverage]
    internal static class NativeMethods
    {
        public const int LOGPIXELSX = 88;

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool CheckMenuItem(IntPtr hMenu, uint uIDCheckItem, uint uCheck);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr CreateToolhelp32Snapshot([In] UInt32 dwFlags, [In] UInt32 th32ProcessID);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr CreateWindowExW(UInt32 dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, UInt32 dwStyle, Int32 x, Int32 y, Int32 nWidth, Int32 nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumThreadDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("User32.dll")]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO mi);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetPerformanceInfo([Out] out PERFORMANCE_INFORMATION pPerformanceInformation, [In] int cb);

        [DllImport("psapi.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetProcessMemoryInfo(IntPtr hProcess, out PROCESS_MEMORY_COUNTERS counters, uint size);

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetProcessTimes(IntPtr hProcess, ref long lpCreationTime, ref long lpExitTime,
            ref long lpKernelTime, ref long lpUserTime);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32", SetLastError = true, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool InsertMenu(IntPtr hmenu, int position, uint flags, UIntPtr itemId, [MarshalAs(UnmanagedType.LPWStr)] string itemText);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("NTDLL.DLL", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS pic,
            ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);

        [DllImport("kernel32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("Ole32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void PropVariantClear(ref PropVariant pvar);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool Process32First([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool Process32Next([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern UInt16 RegisterClassW([In] ref WNDCLASS lpWndClass);

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr SendMessage(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP flags);

        [DllImport("shell32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid,
            [MarshalAs(UnmanagedType.Interface)] out IPropertyStore propertyStore);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "GetClientRect", CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool GetClientRect(IntPtr hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll", EntryPoint = "GetAncestor")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetAncestor(IntPtr hwnd, GAF flags);

        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool SetWindowText(IntPtr hwnd, string lpString);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "SetFocus")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "SetWindowPlacement")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint command);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [SuppressUnmanagedCodeSecurity]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int code, HookProc func, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhook);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr handle, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("clrdump.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Int32 CreateDump(Int32 processId, string fileName, Int32 dumpType, Int32 excThreadId, IntPtr extPtrs);
    }
}