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
using System.Security.Cryptography.X509Certificates;

namespace Paragon.AppPackager
{
    [ExcludeFromCodeCoverage]
    public static class ParagonPackageSigner
    {
        internal static bool Sign(string inputPath, string outputPath, X509Certificate2 certificate)
        {
            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
                throw new ArgumentException("inputPath");
            if (string.IsNullOrEmpty(outputPath))
                throw new Exception("outputPath");
            if (certificate == null)
                throw new Exception("certificate");

            using (Package signedPackage = Package.Open(outputPath, FileMode.CreateNew))
            {
                var uriString = "/" + InnerPackageName + ".pgx";
                var uri = new Uri(uriString, UriKind.Relative);
                var innerPackagePart = signedPackage.CreatePart(uri, "application/zip");
                using (var fileStream = File.Open(inputPath, FileMode.Open, FileAccess.Read))
                {
                    using (var stream = innerPackagePart.GetStream())
                    {
                        var array = new byte[81920];
                        int count;
                        while ((count = fileStream.Read(array, 0, array.Length)) != 0)
                        {
                            stream.Write(array, 0, count);
                        }
                    }
                }

                //Make a list of parts to sign (in this case, only one package)
                var partsToSign = new List<Uri> { innerPackagePart.Uri };
                var dsm = new PackageDigitalSignatureManager(signedPackage)
                {
                    CertificateOption = CertificateEmbeddingOption.InSignaturePart,
                    HashAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256"
                };

                if (dsm.Sign(partsToSign, certificate) != null)
                {
                    Console.WriteLine("Signed package {0} created.", outputPath);
                    return true;
                }
            }
            return false;
        }

        static string InnerPackageName
        {
            get
            {
                return new Guid("3CBA2BD0-8F00-41E4-AF18-508A4F5CABDB").ToString().Replace("-", "");
            }
        }
    }
}
