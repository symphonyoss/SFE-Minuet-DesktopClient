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

        string PackageFilePath { get; }

        Stream GetIcon16();

        Stream GetIcon128();

        PackagePart GetPart(string path);
    }
}