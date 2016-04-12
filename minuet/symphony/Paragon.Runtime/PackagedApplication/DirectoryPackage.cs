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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using Xilium.CefGlue;

namespace Paragon.Runtime.PackagedApplication
{
    /// <summary>
    /// An implementation of <see cref="Package"/> that abstracts a file system directory.
    /// </summary>
    public class DirectoryPackage : Package
    {
        private readonly string _directoryPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryPackage"/> 
        /// </summary>
        /// <param name="directoryPath">
        /// The path to the directory.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The folder specified by <paramref name="directoryPath"/> does not exist.
        /// </exception>
        public DirectoryPackage(string directoryPath)
            : base(FileAccess.Read)
        {
            _directoryPath = directoryPath;
            CefRuntime.Load();
            FilePackagePart.InitCustomContentTypes(Path.Combine(_directoryPath, "[Content_Types].xml"));
        }

        /// <summary>
        /// When overridden in a derived class, creates a new part in the package.
        /// </summary>
        /// <param name="partUri">
        /// The uniform resource identifier (URI) for the part being created.
        /// </param>
        /// <param name="contentType">
        /// The content type of the data stream.
        /// </param>
        /// <param name="compressionOption">
        /// The compression option for the data stream.
        /// </param>
        /// <returns>
        /// The created part.
        /// </returns>
        /// Excluding from code coverage as this function has no usages.
        [ExcludeFromCodeCoverage]
        protected override PackagePart CreatePartCore(Uri partUri, string contentType, CompressionOption compressionOption)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, returns the part addressed by a given URI.
        /// </summary>
        /// <param name="partUri">
        /// The uniform resource identifier (URI) of the part to retrieve.
        /// </param>
        /// <returns>
        /// The requested part; or null, if a part with the specified <paramref name="partUri"/> is not in the package.
        /// </returns>
        /// Excluding from code coverage as this function has no usages.
        [ExcludeFromCodeCoverage]
        protected override PackagePart GetPartCore(Uri partUri)
        {
            var partPath = partUri.ToString().Substring(1);
           
            var fullPath = Path.Combine(_directoryPath, partPath);
            return !File.Exists(fullPath) ? null : new FilePackagePart(this, partUri, fullPath);
        }

        /// <summary>
        /// When overridden in a derived class, deletes a part with a given URI. 
        /// </summary>
        /// <param name="partUri">
        /// The <see cref="P:System.IO.Packaging.PackagePart.Uri"/> of the <see cref="T:System.IO.Packaging.PackagePart"/> to delete.
        /// </param>
        /// Excluding from code coverage as this function has no content
        [ExcludeFromCodeCoverage]
        protected override void DeletePartCore(Uri partUri)
        {
            // Required method override.
        }

        /// <summary>
        /// When overridden in a derived class, returns an array of all the parts in the package. 
        /// </summary>
        /// <returns>
        /// An array of all the parts that are contained in the package.
        /// </returns>
        /// Excluding from code coverage as this function has no usages.
        [ExcludeFromCodeCoverage]
        protected override PackagePart[] GetPartsCore()
        {

            return Directory.GetFiles(_directoryPath, "*.*", SearchOption.AllDirectories)
                .Select(CreateFilePart)
                .OfType<PackagePart>()
                .ToArray();
        }

        /// <summary>
        /// When overridden in a derived class, saves the content of all parts and relationships to the derived class store.
        /// </summary>
        /// Excluding from code coverage as this function has no usages.
        [ExcludeFromCodeCoverage]
        protected override void FlushCore()
        {
            // Required method override.
        }

        /// Excluding from code coverage as this function has no usages.
        [ExcludeFromCodeCoverage]
        private FilePackagePart CreateFilePart(string filePath)
        {
            var path = filePath.Replace(_directoryPath, string.Empty);
            path = path.StartsWith("/") ? path : ("/" + path);
            return new FilePackagePart(this, new Uri(path), filePath);
        }
    }
}