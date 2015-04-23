using System.Diagnostics;

namespace Paragon.Runtime
{
    public static class ParagonTraceSources
    {
        public static readonly TraceSource Default = new TraceSource("Paragon", SourceLevels.All);
        public static readonly TraceSource App = new TraceSource("ParagonApp", SourceLevels.All);
    }
}