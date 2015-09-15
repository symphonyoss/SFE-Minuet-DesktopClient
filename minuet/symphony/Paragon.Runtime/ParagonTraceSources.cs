using System.Diagnostics;

namespace Paragon.Runtime
{
    public static class ParagonTraceSources
    {
        public static readonly TraceSource Default;
        public static readonly TraceSource App = new TraceSource("ParagonApp", SourceLevels.Information);

        static ParagonTraceSources()
        {
            #if DEBUG
                Default = new TraceSource("Paragon", SourceLevels.All);
            #else
                Default = new TraceSource("Paragon", SourceLevels.Information);
            #endif
        }
    }
}