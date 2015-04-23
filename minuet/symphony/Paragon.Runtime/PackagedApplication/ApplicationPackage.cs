using System;
using System.IO;
using System.IO.Packaging;
using System.Security.Permissions;
using Newtonsoft.Json;
using Paragon.Plugins;

namespace Paragon.Runtime.PackagedApplication
{
    public sealed class ApplicationPackage : IApplicationPackage
    {
        private const string PackagedAppManifestFileName = "manifest.json";
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly string _packageFilePath;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public ApplicationPackage(string packageFilePath)
        {
            Package package;
            ApplicationManifest manifest = null;

            packageFilePath = new Uri(packageFilePath).LocalPath;

            if (!string.IsNullOrEmpty(packageFilePath) && 
                packageFilePath.EndsWith(PackagedAppManifestFileName, StringComparison.InvariantCultureIgnoreCase) && 
                File.Exists(packageFilePath))
            {
                packageFilePath = packageFilePath.Replace(PackagedAppManifestFileName, string.Empty);
                package = new DirectoryPackage(packageFilePath);
            }
            else if( !string.IsNullOrEmpty(packageFilePath) && 
                     Directory.Exists(packageFilePath) && 
                     File.Exists(Path.Combine(packageFilePath, PackagedAppManifestFileName)))
            {
                package = new DirectoryPackage(packageFilePath);
            }
            else
            {
                if (string.IsNullOrEmpty(packageFilePath) || !File.Exists(packageFilePath))
                {
                    throw new Exception(string.Format("Application '{0}' does not exist", packageFilePath));
                }

                package = Package.Open(packageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            
            _packageFilePath = packageFilePath;

            var manifestFile = GetPart(package, PackagedAppManifestFileName);
            if (manifestFile != null)
            {
                var fileStream = manifestFile.GetStream();
                using (var reader = new StreamReader(fileStream))
                {
                    var json = reader.ReadToEnd();
                    manifest = JsonConvert.DeserializeObject<ApplicationManifest>(json);

                    var appInfo = manifest.App;
                    var applicationType = ApplicationManifest.GetApplicationType(appInfo);
                    manifest.Type = applicationType;
                }
            }

            Init(package, manifest);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public ApplicationPackage(Package package, IApplicationManifest manifest)
        {
            Init(package, manifest);
        }

        public string PackageFilePath
        {
            get
            {
                return _packageFilePath;
            }
        }

        public Package Package { get; private set; }
        public IApplicationManifest Manifest { get; private set; }

        public Stream GetIcon16()
        {
            if (Manifest.Icons != null)
            {
                return GetStream(!string.IsNullOrEmpty(Manifest.Icons.Icon16)
                    ? Manifest.Icons.Icon16
                    : Manifest.Icons.Icon128);
            }
            return null;
        }

        public Stream GetIcon128()
        {
            if (Manifest.Icons != null)
            {
                return GetStream(!string.IsNullOrEmpty(Manifest.Icons.Icon128)
                    ? Manifest.Icons.Icon128
                    : Manifest.Icons.Icon16);
            }
            return null;
        }

        public PackagePart GetPart(string path)
        {
            return GetPart(Package, path);
        }

        private void Init(Package package, IApplicationManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

            Package = package;
            Manifest = manifest;
        }

        public PackagePart GetPart(Package package, string path)
        {
            if (package == null)
            {
                return null;
            }

            try
            {
                path = path.StartsWith("/") ? path : ("/" + path);
                path = path.Replace("\\", "/");
                var partUri = new Uri(path, UriKind.Relative);
                var exists = package.PartExists(partUri);

                if (exists)
                {
                    return package.GetPart(partUri);
                }

                Logger.Warn(fmt => fmt("Package part not found: " + path));
                return null;
            }
            catch (Exception e)
            {
                Logger.Error(fmt => fmt("Failed to get package part. Path:{0}, Exception: {1}", path, e.ToString()));
                return null;
            }
        }

        private Stream GetStream(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var part = GetPart(path);
                return part != null ? part.GetStream() : null;
            }
            return null;
        }

        public ApplicationPackage ToPackagedApplicationPackage()
        {
            var manifest = Manifest;

            if (manifest == null)
            {
                throw new Exception("No manifest in package");
            }

            var app = manifest.App;
            if (app == null)
            {
                throw new Exception("Missing application info in the manifest");
            }

            var launch = app.Launch;
            if (launch == null)
            {
                throw new Exception("Missing application launch information");
            }

            var bounds = new BoundsSpecification
            {
                Width = launch.Width >= 0 ? launch.Width : 1000,
                Height = launch.Height >= 0 ? launch.Height : 750
            };
            if (launch.Left >= 0)
            {
                bounds.Left = launch.Left;
            }
            if (launch.Top >= 0)
            {
                bounds.Top = launch.Top;
            }
            if (launch.MaxWidth >= 0)
            {
                bounds.MaxWidth = launch.MaxWidth;
            }
            if (launch.MaxHeight >= 0)
            {
                bounds.MaxHeight = launch.MaxHeight;
            }
            if (launch.MinWidth >= 0)
            {
                bounds.MinWidth = launch.MinWidth;
            }
            if (launch.MinHeight >= 0)
            {
                bounds.MinHeight = launch.MinHeight;
            }

            var stream = new MemoryStream();
            var newPackage = Package.Open(stream, FileMode.Create);
            var partUri = PackUriHelper.CreatePartUri(new Uri("background.js", UriKind.Relative));
            var bgScriptPart = newPackage.CreatePart(partUri, "text/javascript");

            var partStream = bgScriptPart.GetStream();
            using (var sr = new StreamWriter(partStream))
            {
                sr.WriteLine(CreateBackgroundScript(app.Launch.WebUrl, bounds));
                sr.Flush();
                partStream.Flush();
            }

            newPackage.Flush();
            newPackage.Close();
            stream.Flush();

            newPackage = Package.Open(stream, FileMode.Open);
            app.Launch = null;
            app.Background = new BackgroundInfo
            {
                Scripts = new[] {"background.js"}
            };
            return new ApplicationPackage(newPackage, manifest);
        }

        private static string CreateBackgroundScript(string url, BoundsSpecification bounds)
        {
            var boundsJson = bounds != null ? (", 'outerBounds' : " + JsonConvert.SerializeObject(bounds)) : string.Empty;
            var background = string.Format(@"
                paragon.app.window.create('{0}', {{
                        'frame': {{
                            'type': 'paragon'
                        }}
                        {1}
                    }});
                ",
                url,
                boundsJson);

            return background;
        }

// ReSharper disable UnusedAutoPropertyAccessor.Local

        private class BoundsSpecification
        {
            public BoundsSpecification()
            {
                Left = double.NaN;
                Top = double.NaN;
                Width = double.NaN;
                Height = double.NaN;
                MinWidth = double.NaN;
                MinHeight = double.NaN;
                MaxWidth = double.NaN;
                MaxHeight = double.NaN;
            }

            public double Left { get; set; }
            public double Top { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public double MinWidth { get; set; }
            public double MinHeight { get; set; }
            public double MaxWidth { get; set; }
            public double MaxHeight { get; set; }
        }

// ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}