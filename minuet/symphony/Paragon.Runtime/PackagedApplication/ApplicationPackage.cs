//Licensed to the Apache Software Foundation(ASF) under one
//or more contributor license agreements.See the NOTICE file
//distributed with this work for additional information
//regarding copyright ownership.The ASF licenses this file
//to you under the Apache License, Version 2.0 (the
//"License"); you may not use this file except in compliance
//with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing,
//software distributed under the License is distributed on an
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//KIND, either express or implied.  See the License for the
//specific language governing permissions and limitations
//under the License.

using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Paragon.Plugins;

namespace Paragon.Runtime.PackagedApplication
{
    public sealed class ApplicationPackage : IApplicationPackage
    {
        private const string PackagedAppManifestFileName = "manifest.json";
        public static string[] DefaultCustomProtocolWhitelist = new string[] { "mailto" };
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly string _packageFilePath;
        private readonly Func<Package> _packageFunc;
        private readonly Timer _closeTimer;
        PackageDigitalSignature _signature;
        private Package _package;
        private Func<Package,Package> _packageSignatureValidator;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public ApplicationPackage(string packageFilePath, Func<Package, Package> packageSignatureValidator)
        {
            if (string.IsNullOrEmpty(packageFilePath))
            {
                throw new ArgumentNullException("packageFilePath");
            }

            if (packageSignatureValidator == null)
            {
                throw new ArgumentNullException("packageSignatureValidator");
            }

            _packageSignatureValidator = packageSignatureValidator;

            //packageFilePath argument passed must be a manifest file, path to a directory which has the manifest file or a pgx package
            var localPath = new Uri(packageFilePath).LocalPath;
            if (File.Exists(localPath))
            {
                if (PackagedAppManifestFileName.Equals(Path.GetFileName(localPath), StringComparison.OrdinalIgnoreCase))
                {
                    //A path to a manifest file was passed in.
                    _packageFilePath = localPath.Replace(PackagedAppManifestFileName, string.Empty);
                    _packageFunc = () =>  new DirectoryPackage(_packageFilePath);
                }
                else
                {
                    var ext = Path.GetExtension(localPath);
                    if (!string.IsNullOrEmpty(ext) && ext.Equals(".pgx", StringComparison.OrdinalIgnoreCase))
                    {
                        //A path to a pgx package was passed in.
                        _packageFilePath = localPath;
                        _packageFunc = () => Package.Open(_packageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown package file: "+localPath);
                    }
                }
            }
            else if (Directory.Exists(localPath))
            {
                //A path to a package directory was passed in.
                var manifestPath = Path.Combine(localPath, PackagedAppManifestFileName);
                if (File.Exists(manifestPath))
                {
                    _packageFilePath = localPath;
                    _packageFunc = () => new DirectoryPackage(_packageFilePath);
                }
                else
                {
                    throw new FileNotFoundException("Manifest file not found: " + manifestPath);
                }
            }
            else
            {
                throw new InvalidOperationException("Application package not found: " + packageFilePath);
            }

            if (Package == null)
                throw new Exception("Could not resolve application package");

            var manifestFile = GetPart(PackagedAppManifestFileName);

            if (manifestFile != null)
            {
                var fileStream = manifestFile.GetStream();
                using (var reader = new StreamReader(fileStream))
                {
                    var json = reader.ReadToEnd();
                    Manifest = JsonConvert.DeserializeObject<ApplicationManifest>(json);
                    SetManifestDefaults();

                    var appInfo = Manifest.App;
                    var applicationType = ApplicationManifest.GetApplicationType(appInfo);
                    Manifest.Type = applicationType;
                }
            }
            else
            {
                throw new InvalidOperationException("Manifest not found: " + PackagedAppManifestFileName);
            }

            _closeTimer = new Timer(_ =>
            {
                if (_package != null)
                {
                    _package.Close();
                    _package = null;
                }
            }, null, 10000, Timeout.Infinite);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public ApplicationPackage(Package package, IApplicationManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

            _package = package;
            Manifest = manifest;
        }

        private Package Package
        {
            get
            {
                if (_package == null)
                {
                    var p = _packageFunc();
                    _signature = p.GetSignature();
                    _package = _packageSignatureValidator(p);
                }

                if (_closeTimer != null)
                {
                    _closeTimer.Change(10000, Timeout.Infinite);
                }

                return _package;
            }
        }

        public string PackageFilePath
        {
            get
            {
                return _packageFilePath;
            }
        }

        public PackageDigitalSignature Signature
        {
            get
            {
                return _signature;
            }
        }

        public IApplicationManifest Manifest { get; private set; }

        public Stream GetIcon()
        {
            try
            {
                if (Manifest.Icons == null)
                {
                    Logger.Info("No icons specified in the app manifest");
                    return null;
                }

                var iconPath = !string.IsNullOrEmpty(Manifest.Icons.Icon128)
                    ? Manifest.Icons.Icon128
                    : Manifest.Icons.Icon16;

                if (string.IsNullOrEmpty(iconPath))
                {
                    Logger.Warn("Icon entry in the manifest has a null or empty path");
                    return null;
                }

                Uri iconUri;
                if (!Uri.TryCreate(iconPath, UriKind.RelativeOrAbsolute, out iconUri))
                {
                    Logger.Warn("Invalid icon URI: " + iconPath);
                    return null;
                }

                if (!iconUri.IsAbsoluteUri)
                {
                    // If the icon is a relative URL, we assume that, it is a file in the package. 
                    return GetStream(iconPath);
                }

                if (iconUri.Scheme.Equals("data", StringComparison.InvariantCultureIgnoreCase))
                {
                    var p = iconUri.PathAndQuery;
                    var data = p.Substring(p.IndexOf(',') + 1);
                    return new MemoryStream(Convert.FromBase64String(data));
                }

                var webRequest = (HttpWebRequest)WebRequest.Create(iconUri);
                webRequest.Method = WebRequestMethods.Http.Get;

                using (var webResponse = webRequest.GetResponse())
                {
                    if (((HttpWebResponse) webResponse).StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = webResponse.GetResponseStream())
                        {
                            if (stream == null)
                            {
                                return null;
                            }

                            var targetStream = new MemoryStream();
                            const int bufSize = 0x1000;
                            var buf = new byte[bufSize];
                            int bytesRead;
                            while ((bytesRead = stream.Read(buf, 0, bufSize)) > 0)
                            {
                                targetStream.Write(buf, 0, bytesRead);
                            }

                            return targetStream;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error retrieving icon", ex);
            }
            return null;
        }

        public PackagePart GetPart(string path)
        {
            try
            {
                path = path.StartsWith("/") ? path : ("/" + path);
                path = path.Replace("\\", "/");
                var partUri = new Uri(path, UriKind.Relative);
                var exists = Package.PartExists(partUri);

                if (exists)
                {
                    return Package.GetPart(partUri);
                }

                Logger.Warn("Package part not found: " + path);
                return null;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to get package part. Path:{0}, Exception: {1}", path, e.ToString());
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

            if (manifest.Type == ApplicationType.Hosted)
            {
                Uri result = null;
                if (Uri.TryCreate(app.Launch.WebUrl, UriKind.RelativeOrAbsolute, out result))
                {
                    if (!result.IsAbsoluteUri)
                        throw new Exception("Missing absolute uri in the manifest");
                }
                else
                {
                    throw new Exception("Invalid app launch web url in the manifest");
                }
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
                sr.WriteLine(CreateBackgroundScript(app, bounds));
                sr.Flush();
                partStream.Flush();
            }
            // Create icons in package
            if (manifest.Icons != null)
            {
                CreateIconPart(newPackage, manifest.Icons.Icon16);
                CreateIconPart(newPackage, manifest.Icons.Icon128);
            }
            else
            {
                // Set up the favicon as the icon for the application
                var uri = new Uri(app.Launch.WebUrl);
                var path = string.IsNullOrEmpty(uri.Query) ? uri.PathAndQuery : uri.PathAndQuery.Substring(0, uri.PathAndQuery.IndexOf(uri.Query));
                path += path.EndsWith("/") ? "favicon.ico" : "/favicon.ico";
                manifest.Icons = new IconInfo() { Icon128 = string.Format("{0}://{1}{2}{3}", uri.Scheme, uri.Host, uri.IsDefaultPort ? string.Empty : (":" + uri.Port.ToString()), path) };
            }

            newPackage.Flush();
            newPackage.Close();
            stream.Flush();

            newPackage = Package.Open(stream, FileMode.Open);
            app.Launch = null;
            app.Background = new BackgroundInfo
            {
                Scripts = new[] { "background.js" }
            };
            return new ApplicationPackage(newPackage, manifest);
        }

        private void CreateIconPart(Package package, string iconPath)
        {
            // No icon defined.
            if (string.IsNullOrEmpty(iconPath))
            {
                return;
            }

            // Icon path exists, try to parse it to a URI.
            Uri iconUri;
            if (!Uri.TryCreate(iconPath, UriKind.RelativeOrAbsolute, out iconUri))
            {
                Logger.Error("Invalid icon URI: " + iconPath);
                return;
            }

            // The URI is a path to an external icon, skip creating the part.
            if (iconUri.IsAbsoluteUri)
            {
                return;
            }

            // We have a valid relative icon URI, get the stream for the icon.
            var stream = GetStream(iconPath);
            if (stream == null)
            {
                Logger.Error("Icon not found at: " + iconPath);
                return;
            }

            CreatePartFromStream(stream, package, "image/png", iconPath);
        }

        private void SetManifestDefaults()
        {
            if (Manifest.CustomProtocolWhitelist == null)
            {
                Manifest.CustomProtocolWhitelist = DefaultCustomProtocolWhitelist;
            }
            else
            {
                if (!Manifest.CustomProtocolWhitelist.Contains("*"))
                {
                    Manifest.CustomProtocolWhitelist = Manifest.CustomProtocolWhitelist.Union(DefaultCustomProtocolWhitelist).ToArray();
                }
            }
        }

        /// <summary>
        /// Creates a new part in the specified package and sets its value to the given stream
        /// </summary>
        /// <param name="source">The source steam to be written to the new part</param>
        /// <param name="target">The target package in which to create the new part</param>
        /// <param name="contentType">The mime type of the new part</param>
        /// <param name="targetPath">The relative path of the new part within the package</param>
        private static void CreatePartFromStream(Stream source, Package target, string contentType, string targetPath)
        {
            var targetPartUri = PackUriHelper.CreatePartUri(new Uri(targetPath, UriKind.Relative));

            var targetPart = target.CreatePart(targetPartUri, contentType);
            using (var targetPartStream = targetPart.GetStream())
            {
                const int bufSize = 0x1000;
                byte[] buf = new byte[bufSize];
                int bytesRead = 0;
                while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
                    targetPartStream.Write(buf, 0, bytesRead);
            }
        }

        private static string CreateBackgroundScript(IAppInfo appInfo, BoundsSpecification bounds)
        {
            var windoInfoJson = new StringBuilder();

            if (!string.IsNullOrEmpty(appInfo.Launch.Id))
            {
                windoInfoJson.AppendFormat(", 'id': '{0}'", appInfo.Launch.Id);
            }

            if (!appInfo.Launch.AutoSaveLocation)
            {
                windoInfoJson.Append(", 'autoSaveLocation': false");
            }

            var boundsJson = bounds != null ? (", 'outerBounds' : " + JsonConvert.SerializeObject(bounds)) : string.Empty;
            var background = string.Format(@"
                paragon.app.window.create('{0}', {{
                        'frame': {{
                            'type': 'notSpecified'
                        }}
                        {1}
                        {2}
                    }});
                ",
                appInfo.Launch.WebUrl,
                windoInfoJson,
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