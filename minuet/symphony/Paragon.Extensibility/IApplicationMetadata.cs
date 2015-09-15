using System;
using System.IO.Packaging;

namespace Paragon.Plugins
{
    public interface IApplicationMetadata : IEquatable<IApplicationMetadata>
    {
        ApplicationType AppType { get; set; }

        ApplicationEnvironment Environment { get; set; }

        string Id { get; set; }

        string InstanceId { get; set; }

        string StartData { get; set; }

        string WorkspaceId { get; set; }

        int WDPort { get; set; }

        Action<string> UpdateLaunchStatus { get; set; }

        bool IsStandalone { get; set; }

        Func<Package,Package> PackageSignatureVerifier { get; set; }

        IApplicationPackage GetApplicationPackage();
    }
}