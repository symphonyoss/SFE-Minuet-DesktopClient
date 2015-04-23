using System.Drawing;

namespace Paragon.Runtime.Kernel.Windowing
{
    public class Bounds
    {
        public int Height;
        public int Left;
        public int Top;
        public int Width;

        public static Bounds FromRectangle(Rectangle r)
        {
            return new Bounds {Left = r.Left, Top = r.Top, Width = r.Width, Height = r.Height};
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            var other = obj as Bounds;
            if (other == null)
            {
                return false;
            }

            if (Left != other.Left)
            {
                return false;
            }

            if (Top != other.Top)
            {
                return false;
            }

            if (Width != other.Width)
            {
                return false;
            }

            return Height == other.Height;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}