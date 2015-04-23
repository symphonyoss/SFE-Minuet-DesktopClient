using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [Flags]
    internal enum WS : uint
    {
        OVERLAPPED = 0x00000000,
        CAPTION = 0x00C00000,
        SYSMENU = 0x00080000,
        THICKFRAME = 0x00040000,
        MINIMIZEBOX = 0x00020000,
        MAXIMIZEBOX = 0x00010000,
        POPUP = 0x80000000,
        OVERLAPPEDWINDOW = (OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX)
    }

    [ExcludeFromCodeCoverage]
    [Flags]
    internal enum WSEX
    {
        TOPMOST = 0x00000008
    }
}