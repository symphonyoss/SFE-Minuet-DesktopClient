using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Paragon.Plugins;

namespace Paragon.Runtime.PackagedApplication
{
    public static class ApplicationPackageResolver
    {
        public static string Download(string packageUri, out string contentType)
        {
            string filePath = null;
            var uri = new Uri(packageUri);
            contentType = string.Empty;

            if (!uri.IsFile && !uri.IsUnc)
            {
                var request = WebRequest.Create(packageUri);
                using (var resp = request.GetResponse())
                {
                    var tempDir = Path.GetTempPath();
                    contentType = resp.ContentType;

                    if (contentType != null)
                    {
                        if (contentType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase))
                        {
                            filePath = Path.Combine(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", "")), "manifest.json");
                            contentType = null;
                        }
                        else if (contentType.Equals("application/zip", StringComparison.InvariantCultureIgnoreCase) ||
                                 contentType.Equals("application/x-zip", StringComparison.InvariantCultureIgnoreCase) ||
                                 contentType.Equals("application/octet-stream", StringComparison.InvariantCultureIgnoreCase) ||
                                 contentType.Equals("application/x-zip-compressed", StringComparison.InvariantCultureIgnoreCase))
                        {
                            filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", "") + ".zip");
                            contentType = null;
                        }
                    }

                    // Download the package or manifest
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        using (var remoteStream = resp.GetResponseStream())
                        {
                            var dir = Path.GetDirectoryName(filePath);

                            if (!Directory.Exists(dir) && !tempDir.Equals(dir, StringComparison.InvariantCultureIgnoreCase))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            using (var localStream = File.Create(filePath))
                            {
                                var buffer = new byte[1024];

                                do
                                {
                                    var bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                                    if (bytesRead > 0)
                                    {
                                        localStream.Write(buffer, 0, bytesRead);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                } while (true);

                                localStream.Flush();
                            }
                        }
                    }
                }
            }

            return filePath ?? packageUri;
        }

        public static ApplicationPackage Load(string packageUri, out string resolvedUri)
        {
            ApplicationPackage package;
            string contentType;

            resolvedUri = Download(packageUri, out contentType);
            if (!string.IsNullOrEmpty(contentType))
            {
                // This is a URL to a hosted application
                var manifest = JsonConvert.DeserializeObject<ApplicationManifest>(
                    string.Format("{{ 'name' : 'Unknown', 'type' : 'Hosted', 'id' : '{0}', 'version' : '1.0.0.0', 'app' : {{ 'launch' : {{ 'web_url' : '{1}' }} }} }}", Guid.NewGuid(), packageUri));

                package = new ApplicationPackage(null, manifest).ToPackagedApplicationPackage();
            }
            else
            {
                package = new ApplicationPackage(resolvedUri);
                if (package.Manifest.Type == ApplicationType.Hosted)
                {
                    package = package.ToPackagedApplicationPackage();
                }
            }

            return package;
        }
    }
}