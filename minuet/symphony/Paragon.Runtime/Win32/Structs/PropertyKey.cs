using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PropertyKey
    {
        public Guid FormatId;
        public int PropertyId;
    }
}