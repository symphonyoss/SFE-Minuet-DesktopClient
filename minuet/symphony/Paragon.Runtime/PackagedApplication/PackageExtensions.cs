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
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace Paragon.Runtime.PackagedApplication
{
    //Excluding from code coverage Temporarily; can include once Digital Signing is implemented
    [ExcludeFromCodeCoverage]
    public static class PackageExtensions
    {
        static string GetInnerPackageName()
        {
            var g = new Guid("3CBA2BD0-8F00-41E4-AF18-508A4F5CABDB");
            var innerPackageName = g.ToString().Replace("-", "");
            return innerPackageName;
        }

        public static bool IsSigned(this Package package)
        {
            //VALIDATE THAT THE PACKAGE GIVEN IS A SIGNED PARAGON PACKAGE
            var dsm = new PackageDigitalSignatureManager(package);
            var innerPackageName = GetInnerPackageName();
            var uriString = "/" + innerPackageName + ".pgx";
            var uri = new Uri(uriString, UriKind.Relative);
            return package.PartExists(uri) && dsm.Signatures.Count == 1;
        }

        public static PackageDigitalSignature GetSignature(this Package package)
        {
            //VALIDATE THAT THE PACKAGE GIVEN IS A SIGNED PARAGON PACKAGE
            var dsm = new PackageDigitalSignatureManager(package);
            return package.IsSigned() ? dsm.Signatures[0] : null;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static Package Verify(this Package package)
        {
            if (!IsSigned(package))
            {
                return package;
            }

            // Create the PackageDigitalSignatureManager
            var dsm = new PackageDigitalSignatureManager(package)
                      {
                        HashAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256"
                      };

            // Verify the collection of certificates in the package (one, in this case)
            foreach (PackageDigitalSignature signature in dsm.Signatures)
            {
                var x = PackageDigitalSignatureManager.VerifyCertificate(signature.Signer);

                if (x == X509ChainStatusFlags.NoError)
                    continue;
                // If the certificate has expired, but signature is created while the certificate was valid, we accept it
                else if( x == X509ChainStatusFlags.NotTimeValid )
                {
                    var signer = signature.Signer as X509Certificate2;
                    if( signer != null && 
                        signature.SigningTime >= signer.NotBefore &&
                        signature.SigningTime <= signer.NotAfter )
                        continue;
                }
                throw new Exception(string.Format("Certificate validation failed : {0}", x));
            }

            // If all certificates are valid, verify all signatures in the package.
            VerifyResult vResult = dsm.VerifySignatures(false);

            //Retrieve innerPackage/originalPackage
            var innerPackageName = GetInnerPackageName();
            var uriString = "/" + innerPackageName + ".pgx";
            var packagePart = package.GetPart(new Uri(uriString, UriKind.Relative));
            var stream = packagePart.GetStream();
            var originalPackage = Package.Open(stream);
            return originalPackage;
        }
    }
}