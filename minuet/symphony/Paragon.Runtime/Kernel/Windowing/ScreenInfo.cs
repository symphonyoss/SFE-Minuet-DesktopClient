// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Paragon.Runtime.Kernel.Windowing
{
    public sealed class ScreenInfo
    {
        public string Name { get; set; }

        public bool IsPrimary { get; set; }

        public Bounds Bounds { get; set; }

        public Bounds WorkArea { get; set; }

        private bool Equals(ScreenInfo other)
        {
            return string.Equals(Name, other.Name);
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
            return obj is ScreenInfo && Equals((ScreenInfo) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(ScreenInfo left, ScreenInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScreenInfo left, ScreenInfo right)
        {
            return !Equals(left, right);
        }
    }
}