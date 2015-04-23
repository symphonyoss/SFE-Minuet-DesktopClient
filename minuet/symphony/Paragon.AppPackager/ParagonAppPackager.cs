using System;
using System.Collections.Generic;
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

        public static bool Package(string inputFolder, string outputPath)
        {
            var sourceFolder = PathValidation.GetValidInputFolder(inputFolder);
            if (sourceFolder == null)
            {
                throw new Exception("Unable to validate given InputPath.");
            }

            if (!JsonContentValidation.ValidateJsonFileContent(Path.Combine(sourceFolder, "manifest.json")))
            {
                throw new Exception("Unable to validate contents of the json file. Please add the required fields in the file, and try again");
            }

            var destinationPath = outputPath;
            if (string.IsNullOrEmpty(destinationPath))
            {
                destinationPath = sourceFolder;
            }

            if (!Path.IsPathRooted(destinationPath))
            {
                destinationPath = Path.Combine(Environment.CurrentDirectory, destinationPath);
            }

            var packageDir = Directory.GetParent(destinationPath).FullName;
            if (!Directory.Exists(packageDir))
            {
                Directory.CreateDirectory(packageDir);
            }

            var ext = Path.GetExtension(destinationPath);

            if (string.IsNullOrEmpty(ext))
            {
                destinationPath += ".pgx";
            }
            else
            {
                destinationPath = Path.Combine(packageDir, Path.GetFileName(destinationPath)) + ".pgx";
            }

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

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

                    else
                    {
                        Console.WriteLine("Packing " + currentFile);

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
            return true;
        }

        public static bool PackageAndSign(string inputPath, string outputPath, string certName)
        {
            var unsignedPackagePath = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetTempFileName() + ".pgx");

            try
            {
                if (Package(inputPath, unsignedPackagePath))
                {
                    return ParagonPackageSigner.Sign(unsignedPackagePath, outputPath, certName);
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

            var type = CefRuntime.GetMimeTypeForExtension(ext);
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