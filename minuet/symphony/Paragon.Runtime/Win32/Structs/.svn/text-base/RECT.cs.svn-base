using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;

// ReSharper disable InconsistentNaming

namespace Paragon.Runtime.Win32
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
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

        public Point Location
        {
            get { return new Point(Left, Top); }
            set { Offset(value.X - Left, value.Y - Top); }
        }

        public static RECT FromHandle(IntPtr hwnd)
        {
            RECT rect;
            NativeMethods.GetWindowRect(hwnd, out rect);
            return rect;
        }

        public void Offset(double x, double y)
        {
            var ix = (int) x;
            var iy = (int) y;

            Left += ix;
            Right += ix;
            Top += iy;
            Bottom += iy;
        }

        public bool Equals(RECT other)
        {
            return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is RECT && Equals((RECT) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Left;
                hashCode = (hashCode*397) ^ Top;
                hashCode = (hashCode*397) ^ Right;
                hashCode = (hashCode*397) ^ Bottom;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Top: {0}; Left: {1}; Right: {2}; Bottom: {3}", Top, Left, Right, Bottom);
        }

        public static bool operator ==(RECT left, RECT right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RECT left, RECT right)
        {
            return !left.Equals(right);
        }
    }
}