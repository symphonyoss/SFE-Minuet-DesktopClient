using System.Diagnostics;

namespace Paragon.Runtime
{
    public static class ParagonTraceSources
    {
        public static readonly TraceSource Default = new TraceSource("Paragon", SourceLevels.Information);
        public static readonly TraceSource App = new TraceSource("ParagonApp", SourceLevels.Information);
    }
}