using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using Paragon.Runtime.Kernel.Applications;
using Paragon.Runtime.PackagedApplication;

namespace Paragon.AppPackager
{
    internal static class Program
    {
        [ExcludeFromCodeCoverage]
        public static void Main(string[] args)
        {
            var info = PackagingInfo.Instance;

            try
            {
                if (info == null)
                    throw new Exception("Input error.");

                if (info.ShouldPackage && !ParagonAppPackager.Package(info.InputPath, info.UnsignedPackagePath) )
                    throw new Exception("Error in packaging.");

                if (info.ShouldSign)
                {
                    if (!ParagonPackageSigner.Sign(info.UnsignedPackagePath, info.OutputPackagePath, info.Cert))
                        throw new Exception("Error in signing package.");
                }
                else if (info.ShouldPackage)
                    File.Copy(info.UnsignedPackagePath, info.OutputPackagePath, true);

                if (info.ShouldVerify)
                {
                    using (var signedPackage = Package.Open(info.OutputPackagePath, FileMode.Open, FileAccess.Read))
                    {
                        if( signedPackage.Verify() == null )
                            throw new Exception("Error in verifying the package.");
                    }
                    Console.WriteLine("Signed package {0} is verified.", info.OutputPackagePath);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed : {0}", ex);
                Environment.Exit(-1);
            }
            finally
            {
                if (info.DeleteUnsignedPackage && File.Exists(info.UnsignedPackagePath))
                    File.Delete(info.UnsignedPackagePath);
            }
        }
    }
}