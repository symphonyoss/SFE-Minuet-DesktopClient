using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Paragon.Plugins
{
    public interface IApplicationManager
    {
        event EventHandler AllApplicationsClosed;

        bool ExplicitShutdown { get; }
        string ProcessGroup { get; }
        string CacheFolder { get; }
        ApplicationEnvironment Environment { get; }
        IApplication[] AllApplicaions { get; }
        bool IsInitialized { get; }
        string BrowserLanguage { get; }
        bool DisableSpellChecking { get; }
    }
}
