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