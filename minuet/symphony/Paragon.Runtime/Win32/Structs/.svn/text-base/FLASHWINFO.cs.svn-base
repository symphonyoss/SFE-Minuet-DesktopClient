using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential)]
    internal struct FLASHWINFO
    {
        public uint cbSize; //The size of the structure in bytes.            
        public IntPtr hwnd; //A Handle to the Window to be Flashed. The window can be either opened or minimized.            
        public uint dwFlags; //The Flash Status.            
        public uint uCount; // number of times to flash the window            
        public uint dwTimeout; //The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.        
    }
}