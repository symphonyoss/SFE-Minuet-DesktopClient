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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Xilium.CefGlue;

namespace Paragon.AppPackager
{
    internal static class ParagonAppPackager
    {
        private static Dictionary<string, string> _contentTypesGiven = new Dictionary<string, string>();

        public static bool Package(string sourceFolder, string destinationPath)
        {
            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder) || !File.Exists(Path.Combine(sourceFolder, "manifest.json")))
            {
                throw new ArgumentException("Invalid input folder.");
            }

            if (!JsonContentValidation.ValidateJsonFileContent(Path.Combine(sourceFolder, "manifest.json")))
            {
                throw new Exception("Application manifest is invalid");
            }

            var packageDir = Directory.GetParent(destinationPath).FullName;

            if (!Directory.Exists(packageDir))
            {
                Directory.CreateDirectory(packageDir);
            }

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            Console.WriteLine("Input folder: " + sourceFolder);

            using (var package = System.IO.Packaging.Package.Open(destinationPath, FileMode.Create))
            {
                foreach (var currentFile in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
                {
                    if (currentFile.EndsWith("\\[Content_Types].xml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var doc = XDocument.Load(currentFile);
                        if (doc.Root == null)
                        {
                            throw new Exception("[Content_Types].xml document provided could not be loaded");
                        }

                        _contentTypesGiven = doc.Root.Elements().ToDictionary(a => "." + (string) a.Attribute("Extension"), a => (string) a.Attribute("ContentType"));
                    }
                    else if( !destinationPath.Equals(currentFile) )
                    {
                        var uri = GetRelativeUri(currentFile.Replace(sourceFolder, string.Empty));
                        var contentType = GetContentType(currentFile);
                        var packagePart = package.CreatePart(uri, contentType, CompressionOption.Maximum);

                        using (var fileStream = new FileStream(currentFile, FileMode.Open, FileAccess.Read))
                        using (var packageStream = packagePart.GetStream())
                        {
                            var array = new byte[81920];
                            int count;
                            while ((count = fileStream.Read(array, 0, array.Length)) != 0)
                            {
                                packageStream.Write(array, 0, count);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Unsigned package {0} created.", destinationPath);
            return true;
        }

        //Yet to implement this perfectly; excluding from code coverage
        [ExcludeFromCodeCoverage]
        public static bool PackageAndSign(string inputPath, string outputPath, string certName)
        {
            var unsignedPackagePath = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetTempFileName() + ".pgx");

            try
            {
                if (Package(inputPath, unsignedPackagePath))
                {
                    return ParagonPackageSigner.Sign(unsignedPackagePath, outputPath, PackagingInfo.FindCertificate(certName, true));
                }
            }
            finally
            {
                if (File.Exists(unsignedPackagePath))
                {
                    File.Delete(unsignedPackagePath);
                }
            }

            return false;
        }

        private static string GetContentType(string packageFile)
        {
            var ext = Path.GetExtension(packageFile);

            if (_contentTypesGiven.Count > 0 && _contentTypesGiven.ContainsKey(ext))
            {
                return _contentTypesGiven[ext];
            }

            var type = CefRuntime.GetMimeType(ext);
            if (type != null)
            {
                return type;
            }

            switch (ext)
            {
                case ".html":
                case ".htm":
                    return "text/html";

                case ".js":
                    return "text/javascript";

                case ".css":
                    return "text/css";

                case ".json":
                    return "application/json";

                case ".png":
                    return "image/png";

                case ".gif":
                    return "image/gif";

                case ".ico":
                    return "image/vnd.microsoft.icon";

                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";

                case ".mp3":
                    return "audio/mpeg3";

                case ".xml":
                    return "application/xml";

                case ".svg":
                    return "image/svg+xml";

                case ".ttf":
                    return "application/x-font-ttf";

                case ".eot":
                    return "application/vnd.ms-fontobject";

                case ".woff":
                    return "application/x-woff";

                default:
                    return MediaTypeNames.Application.Octet;
            }
        }

        private static Uri GetRelativeUri(string currentFile)
        {
            var uriString = currentFile;
            if (!uriString.StartsWith("\\"))
            {
                uriString = "\\" + uriString;
            }

            var relPath = uriString.Substring(
                uriString.IndexOf('\\')).Replace('\\', '/').Replace(' ', '_');

            var normalized = relPath.Normalize(NormalizationForm.FormKD);

            var removal = Encoding.GetEncoding(
                Encoding.ASCII.CodePage,
                new EncoderReplacementFallback(string.Empty),
                new DecoderReplacementFallback(string.Empty));

            var bytes = removal.GetBytes(normalized);
            var uri = Encoding.ASCII.GetString(bytes);
            return new Uri(uri, UriKind.Relative);
        }
    }
}