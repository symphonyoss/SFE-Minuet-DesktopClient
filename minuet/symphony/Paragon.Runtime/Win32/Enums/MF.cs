using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [Flags]
    internal enum MF
    {
        CHECKED = 0x8,
        UNCHECKED = 0x0,
        SEPARATOR = 0x800,
        BYCOMMAND = 0x0,
        BYPOSITION = 0x400,
        STRING = 0x0,
        ENABLED = 0x0,
        DISABLED = 0x2
    }
}