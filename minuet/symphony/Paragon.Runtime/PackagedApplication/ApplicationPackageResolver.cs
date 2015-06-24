using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Paragon.Plugins;

namespace Paragon.Runtime.PackagedApplication
{
    public static class ApplicationPackageResolver
    {
        public static IApplicationPackage Load(string packageUri, out string resolvedUri)
        {
            ApplicationPackage package;
            bool isHosted;

            if (!Download(packageUri, out resolvedUri, out isHosted))
            {
                return null;
            }

            if (isHosted)
            {
                // This is a URL to a hosted application
                const string json = "{{ 'name' : 'Unknown', 'type' : 'Hosted', 'id' : '{0}', 'version' : '1.0.0.0', 'app' : {{ 'launch' : {{ 'web_url' : '{1}' }} }} }}";
                var formattedJson = string.Format(json, Guid.NewGuid(), packageUri);
                var manifest = JsonConvert.DeserializeObject<ApplicationManifest>(formattedJson);
                package = new ApplicationPackage(null, manifest);
            }
            else
            {
                package = new ApplicationPackage(resolvedUri);
            }

            return package.Manifest.Type == ApplicationType.Hosted 
                ? package.ToPackagedApplicationPackage() 
                : package;
        }

        private static bool Download(string packageUri, out string resolvedUri, out bool isHosted)
        {
            var uri = new Uri(packageUri);
            if (uri.IsFile || uri.IsUnc)
            {
                // The URI is to a file on a local or UNC drive. Use that for the resolved URI.
                resolvedUri = packageUri;
                isHosted = false;
                return true;
            }

            try
            {
                var request = WebRequest.Create(packageUri);

                using (var resp = request.GetResponse())
                {
                    var contentType = resp.ContentType;
                    if (contentType == null)
                    {
                        resolvedUri = packageUri;
                        isHosted = true;
                        return true;
                    }

                    string filePath;
                    if (contentType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase))
                    {
                        filePath = Path.Combine(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", "")), "manifest.json");
                    }
                    else if (contentType.Equals("application/zip", StringComparison.InvariantCultureIgnoreCase) ||
                             contentType.Equals("application/x-zip", StringComparison.InvariantCultureIgnoreCase) ||
                             contentType.Equals("application/octet-stream", StringComparison.InvariantCultureIgnoreCase) ||
                             contentType.Equals("application/x-zip-compressed", StringComparison.InvariantCultureIgnoreCase))
                    {
                        filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", "") + ".pgx");
                    }
                    else
                    {
                        resolvedUri = packageUri;
                        isHosted = true;
                        return true;
                    }

                    // Download the package or manifest
                    using (var responseStream = resp.GetResponseStream())
                    {
                        if (responseStream == null)
                        {
                            resolvedUri = null;
                            isHosted = false;
                            return false;
                        }

                        var parentDir = Path.GetDirectoryName(filePath);
                        if (string.IsNullOrEmpty(parentDir))
                        {
                            resolvedUri = null;
                            isHosted = false;
                            return false;
                        }

                        if (!Directory.Exists(parentDir))
                        {
                            Directory.CreateDirectory(parentDir);
                        }

                        using (var outStream = File.Create(filePath))
                        {
                            var buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                outStream.Write(buffer, 0, bytesRead);
                            }

                            outStream.Flush();
                        }
                    }

                    resolvedUri = filePath;
                    isHosted = true;
                    return true;
                }
            }
            catch (Exception)
            {
                resolvedUri = packageUri;
                isHosted = true;
                return true;
            }
        }
    }
}