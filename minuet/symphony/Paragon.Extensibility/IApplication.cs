using System;
using System.Collections.Generic;

namespace Paragon.Plugins
{
    public interface IApplication : IDisposable
    {
        ILogger Logger { get; }
        IApplicationManager ApplicationManager { get; }
        Dictionary<string, object> Args { get; }
        ApplicationState State { get; }
        IApplicationPackage Package { get; }
        string Name { get; }
        string WorkspaceId { get; }
        IApplicationMetadata Metadata { get; }
        IApplicationWindowManager WindowManager { get; }
        List<IParagonPlugin> Plugins { get; }
        event EventHandler<ApplicationExitingEventArgs> Exiting;
        event EventHandler<ProtocolInvocationEventArgs> ProtocolInvoke;
        event EventHandler Launched;
        event EventHandler Closed;
        IApplicationWindow FindWindow(int browserId);
        void Launch();
        void Close();
        IEnumerable<IParagonAppInfo> GetRunningApps();
        void OnProtocolInvoke(string uri);

        bool SetCookie(string name, string value, string domain, string path,
            bool httpOnly, bool secure, DateTime? expires, bool global);
    }
}