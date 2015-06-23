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
    public static class ParagonPackageVerifier
    {
        static string GetInnerPackageName()
        {
            var g = new Guid("3CBA2BD0-8F00-41E4-AF18-508A4F5CABDB");
            var innerPackageName = g.ToString().Replace("-", "");
            return innerPackageName;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool Verify(string signedPackagePath)
        {
            using (var signedPackage = Package.Open(signedPackagePath,FileMode.Open, FileAccess.Read))
            {
                using (var originalPackage = Verify(signedPackage))
                {
                    return (originalPackage != null && originalPackage != signedPackage);
                }
            }
        }

        public static bool IsSignedPackage(Package package)
        {
            //VALIDATE THAT THE PACKAGE GIVEN IS A SIGNED PARAGON PACKAGE
            var dsm = new PackageDigitalSignatureManager(package);
            var innerPackageName = GetInnerPackageName();
            var uriString = "/" + innerPackageName + ".pgx";
            var uri = new Uri(uriString, UriKind.Relative);
            return package.PartExists(uri) && dsm.Signatures.Count == 1;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static Package Verify(Package package)
        {
            if (!IsSignedPackage(package))
            {
                return package;
            }

            // Create the PackageDigitalSignatureManager
            var dsm = new PackageDigitalSignatureManager(package);

            // Verify the collection of certificates in the package (one, in this case)
            foreach (PackageDigitalSignature signature in dsm.Signatures)
            {
                var x = PackageDigitalSignatureManager.VerifyCertificate(signature.Signer);
                if (x != X509ChainStatusFlags.NoError)
                {
                    throw new Exception("Certificate has the following issue:" + x);
                }
            }

            // If all certificates are valid, verify all signatures in the package.
            VerifyResult vResult = dsm.VerifySignatures(false);
            Console.WriteLine("Result of verifying all signatures: " + vResult);

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