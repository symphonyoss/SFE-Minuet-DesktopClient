using System.Security.Permissions;

namespace Paragon.Plugins
{
    public interface IParagonAppInfo
    {
        string AppId { get; }
        string AppInstanceId { get; }
        int BrowserPid { get; }
        string WorkspaceId { get; set; }
        void Dispose();
        int GetHashCode();
    }
}