using System;
using System.Diagnostics.CodeAnalysis;

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [Flags]
    public enum ResizeDirection
    {
        None = 0,
        Bottom = 1,
        Left = 2,
        Right = 4,
        Top = 8
    }
}