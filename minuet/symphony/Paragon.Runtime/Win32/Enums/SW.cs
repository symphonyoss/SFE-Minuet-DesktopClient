using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [Flags]
    public enum SW : uint
    {
        FORCEMINIMIZE = 0x0000000B,
        HIDE = 0x00000000,
        MAXIMIZE = 0x00000003,
        MINIMIZE = 0x00000006,
        RESTORE = 0x00000009,
        SHOW = 0x00000005,
        SHOWDEFAULT = 0x0000000A,
        SHOWMAXIMIZED = 0x00000003,
        SHOWMINIMIZED = 0x00000002,
        SHOWMINNOACTIVE = 0x00000007,
        SHOWNA = 0x00000008,
        SHOWNOACTIVATE = 0x00000004,
        SHOWNORMAL = 0x00000001
    }
}