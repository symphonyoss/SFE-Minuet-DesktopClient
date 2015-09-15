using System;
using System.IO;
using System.IO.Packaging;
using System.Net;
using Newtonsoft.Json;
using Paragon.Plugins;

namespace Paragon.Runtime.PackagedApplication
{
    public static class ApplicationPackageResolver
    {
        public static IApplicationPackage Load(string packageUri, Func<Package,Package> packageValidator, out string resolvedUri)
        {
            ApplicationPackage package = null;
            bool isHosted;

            resolvedUri = Download(packageUri, out isHosted);

            if (!string.IsNullOrEmpty(resolvedUri))
            {
                package = new ApplicationPackage(resolvedUri, packageValidator);
            }

            return package != null && package.Manifest.Type == ApplicationType.Hosted 
                    ? package.ToPackagedApplicationPackage() 
                    : package;
        }

        private static string Download(string packageUri, out bool isHosted)
        {
            isHosted = false;

            try
            {
                var uri = new Uri(packageUri);
                if (!uri.IsFile && !uri.IsUnc)
                {
                    var request = WebRequest.Create(packageUri);

                    using (var resp = request.GetResponse())
                    {
                        var contentType = resp.ContentType;

                        string filePath = null;
                        if (!string.IsNullOrEmpty(contentType) &&
                            (contentType.Equals("application/zip", StringComparison.InvariantCultureIgnoreCase) ||
                             contentType.Equals("application/x-zip", StringComparison.InvariantCultureIgnoreCase) ||
                             contentType.Equals("application/octet-stream", StringComparison.InvariantCultureIgnoreCase) ||
                             contentType.Equals("application/x-zip-compressed", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", "") + ".pgx");
                        }

                        // Download the package or manifest
                        using (var responseStream = resp.GetResponseStream())
                        {
                            var parentDir = Path.GetDirectoryName(filePath);

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

                        return filePath;
                    }
                }
            }
            catch
            {
                return string.Empty;
            }

            return packageUri;
        }
    }
}