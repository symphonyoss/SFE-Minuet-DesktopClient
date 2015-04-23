using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Net.Mime;
using System.Linq;
using System.Xml.Linq;
using Xilium.CefGlue;

namespace Paragon.Runtime.PackagedApplication
{
    /// <summary>
    /// An implementation of <see cref="PackagePart"/> that streams a file.
    /// </summary>
    public class FilePackagePart : PackagePart
    {
        private readonly string _fullPath;

        private static Dictionary<string, string> _contentTypesByFileEnding = new Dictionary<string, string>
        {
            {".txt", "text/plain"},
            {".js", "application/javascript"},
            {".css", "text/css"},
            {".png", "image/png"},
            {".json", "application/json"},
            {".html", "text/html"},
            {".htm", "text/html"},
            {".gif", "image/gif"},
            {".ico", "image/vnd.microsoft.icon"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".mp3", "audio/mpeg3"},
            {".xml", "application/xml"},
            {".svg", "image/svg+xml"},
            {".dll", "application/octet-stream"},
            {".ttf", "application/x-font-ttf"},
            {".eot", "application/vnd.ms-fontobject"},
            {".woff", "application/x-woff"}
        };
        
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.Packaging.PackagePart"/> class 
        /// with a specified parent <see cref="P:System.IO.Packaging.PackagePart.Package"/> and part URI.
        /// </summary>
        /// <param name="package">
        ///     The parent <see cref="T:System.IO.Packaging.Package"/> of the part.
        /// </param>
        /// <param name="partUri">
        ///     The URI of the part, relative to the parent <see cref="T:System.IO.Packaging.Package"/> root.
        /// </param>
        /// <param name="fullPath"></param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="package"/> or <paramref name="partUri"/> is null.
        /// </exception>
        public FilePackagePart(Package package, Uri partUri, string fullPath)
            : base(package, partUri)
        {
            _fullPath = fullPath;
        }

        /// <summary>
        /// When overridden in a derived class, returns the part content stream opened with a
        /// specified <see cref="T:System.IO.FileMode"/> and <see cref="T:System.IO.FileAccess"/>.
        /// </summary>
        /// <param name="mode">
        /// The I/O mode in which to open the content stream.
        /// </param>
        /// <param name="access">
        /// The access permissions to use in opening the content stream.
        /// </param>
        /// <returns>
        /// The content data stream of the part.
        /// </returns>
        protected override Stream GetStreamCore(FileMode mode, FileAccess access)
        {
            using (var fs = new FileStream(_fullPath, mode, access))
            {
                var ms = new MemoryStream();

                var buffer = new byte[2048];
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                ms.Position = 0;
                return ms;
            }
        }

        protected override string GetContentTypeCore()
        {
            var extension = Path.GetExtension(_fullPath).ToLower();
            string contentType = CefRuntime.GetMimeTypeForExtension(extension);
            if (string.IsNullOrEmpty(contentType))
            {
                _contentTypesByFileEnding.TryGetValue(extension, out contentType);
            }
            return string.IsNullOrEmpty(contentType) ? MediaTypeNames.Application.Octet : contentType;
        }

        public static void InitCustomContentTypes(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var doc = XDocument.Load(filePath);
                    var contentTypesGiven = doc.Root.Elements().ToDictionary(a => "." + (string)a.Attribute("Extension"), a => (string)a.Attribute("ContentType"));
                    if (contentTypesGiven != null)
                    {
                        foreach (var key in contentTypesGiven.Keys)
                        {
                            _contentTypesByFileEnding[key.ToLower()] = contentTypesGiven[key];
                        }
                    }
                }
                catch
                {
                    // No need to log
                }
            }
        }
    }
}