using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography.X509Certificates;

namespace Paragon.AppPackager
{
    public static class ParagonPackageSigner
    {
        internal static bool Sign(string inputPath, string outputPath, string certificateName)
        {
            X509Certificate2 certificate = GetCertificate(certificateName);
            if (certificate == null)
            {
                throw new Exception("Could not locate certificate");
            }

            var innerPackageName = GetInnerPackageName();
            using (Package signedPackage = Package.Open(outputPath, FileMode.Create))
            {
                var uriString = "/" + innerPackageName + ".pgx";
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
                    CertificateOption = CertificateEmbeddingOption.InSignaturePart
                };

                return dsm.Sign(partsToSign, certificate) != null;
            }
        }

        static X509Certificate2 GetCertificate(string certificateName)
        {
            var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certs =
                store.Certificates.Find(X509FindType.FindBySubjectName, certificateName, false)
                    .Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, false);

            if (certs.Count != 1)
            {
                throw new Exception("Store contained none/more than one certificate of the specified condition");
            }

            return certs[0];
        }


        static string GetInnerPackageName()
        {
            return new Guid("3CBA2BD0-8F00-41E4-AF18-508A4F5CABDB").ToString().Replace("-", "");
        }
    }
}
