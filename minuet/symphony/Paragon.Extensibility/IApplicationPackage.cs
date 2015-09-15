using System.IO;
using System.IO.Packaging;

namespace Paragon.Plugins
{
    /// <summary>
    /// Defines access to a packaged application deployment.
    /// </summary>
    public interface IApplicationPackage
    {
        IApplicationManifest Manifest { get; }

        PackageDigitalSignature Signature { get; }

        string PackageFilePath { get; }

        Stream GetIcon();

        PackagePart GetPart(string path);
    }
}