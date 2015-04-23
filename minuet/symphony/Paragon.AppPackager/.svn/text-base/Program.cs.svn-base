using System;
using System.Diagnostics;
using Paragon.Runtime.Kernel.Applications;

namespace Paragon.AppPackager
{
    internal static class Program
    {
        private const string InputPath  = "i";
        private const string OutputPath = "o";
        private const string CertificateName = "c";

        public static void Main(string[] args)
        {
            var cmdLine = new ParagonCommandLineParser(args);

            // Launch a debugger if a --debug flag was passed.
            if (cmdLine.HasFlag("debug"))
            {
                Debugger.Launch();
            }

            string inputPath;
            cmdLine.GetValue(InputPath, out inputPath);

            string outputPath;
            cmdLine.GetValue(OutputPath, out outputPath);

            string certificateName;
            cmdLine.GetValue(CertificateName, out certificateName);

            if (string.IsNullOrEmpty(inputPath))
            {
                PrintUsage();
                Environment.ExitCode = 1;
                return;
            }

            if (string.IsNullOrEmpty(certificateName))
            {
                if (!ParagonAppPackager.Package(inputPath, outputPath))
                {
                    Console.WriteLine("Packaging falied.");
                    Environment.ExitCode = 1;
                }
            }
            else
            {
                if (!ParagonAppPackager.PackageAndSign(inputPath, outputPath, certificateName))
                {
                    Console.WriteLine("Packaging and Signing falied.");
                    Environment.ExitCode = 1;
                }
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine(
    @"Missing required argument.\nTo call AppPackager, specify 4 arguments:

    --i:INPUT PATH - The Input path of the folder to be packaged (REQUIRED)

    --o:OUTPUT PATH - The Output path (with the filename) of the packaged folder and signed package (OPTIONAL)
    
    --c:CERTIFICATE NAME - The Certificate name using which the package is signed (OPTIONAL)");
        }
    }
}