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
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Paragon.Runtime.Kernel.Applications;

namespace Paragon.AppPackager
{
    class PackagingInfo
    {
        private static object _lock = new object();
        private static PackagingInfo _inst;

        private PackagingInfo() {}

        public static PackagingInfo Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_inst == null)
                    {
                        var inst = new PackagingInfo();
                        var p = new ParagonCommandLineParser(Environment.GetCommandLineArgs());

                        //Determine the PodUrl to update.
                        string podUrl = null;

                        //Determine if podurl should be updated
                        if (p.GetValue("update", out podUrl) || p.GetValue("u", out podUrl))
                            inst.ShouldUpdateUrl = true;

                        //Determine the PgxPath to update.
                        string pgxPath = null;
                        p.GetValue("pgx", out pgxPath);
                        if (string.IsNullOrEmpty(pgxPath))
                            p.GetValue("p", out pgxPath);
                        inst.PgxPath = pgxPath;

                        //Determine if podurl is valid
                        Uri poduri = null;
                        try
                        {
                            poduri = new Uri(podUrl);
                        }
                        catch (UriFormatException) 
                        {
                            //try to add https schema.
                            if (!String.IsNullOrEmpty(podUrl))
                              poduri = new Uri(String.Concat("https://", podUrl));
                        }
                        inst.PodUrlToUpdate = poduri;

                        // Determine the input path
                        string inputPath = null;
                        p.GetValue("input", out inputPath);
                        if( string.IsNullOrEmpty(inputPath) )
                            p.GetValue("i", out inputPath);
                        if( string.IsNullOrEmpty(inputPath) )
                            inputPath = Directory.GetCurrentDirectory();

                        inst.InputPath = inputPath;

                        // Determine if packaging should be done
                        inst.ShouldPackage = Directory.Exists(inputPath);

                        // Determine the signing certificate
                        string certificate = null;
                        p.GetValue("cert", out certificate);
                        if (string.IsNullOrEmpty(certificate))
                            p.GetValue("c", out certificate);
                        if (!string.IsNullOrEmpty(certificate))
                            inst.Cert = FindCertificate(certificate, true);

                        // Determine if signing should be done
                        inst.ShouldSign = inst.Cert != null && (inst.ShouldPackage || (inst.InputPath.EndsWith(".pgx", StringComparison.InvariantCultureIgnoreCase) && File.Exists(inst.InputPath)));

                        // Determine the input path
                        string outputPath = null;
                        p.GetValue("output", out outputPath);
                        if (string.IsNullOrEmpty(outputPath))
                            p.GetValue("o", out outputPath);

                        if (string.IsNullOrEmpty(outputPath))
                        {
                            if (inst.ShouldPackage)
                                outputPath = Path.Combine(inst.InputPath, Path.GetFileName(inst.InputPath) + ".pgx");
                            else if (inst.ShouldSign)
                                outputPath = Path.Combine(Path.GetDirectoryName(inst.InputPath), Path.GetFileNameWithoutExtension(inst.InputPath) + "_Signed.pgx");
                            else
                                outputPath = inst.InputPath;
                        }

                        if (Directory.Exists(outputPath))
                        {
                            if (inst.ShouldPackage)
                                outputPath = Path.Combine(outputPath, Path.GetFileName(inst.InputPath) + ".pgx");
                            else if (inst.ShouldSign)
                                outputPath = Path.Combine(outputPath, Path.GetFileName(inst.InputPath));
                            else
                                outputPath = inst.InputPath;
                        }
                        else if( !outputPath.EndsWith(".pgx") )
                            outputPath = outputPath + ".pgx";

                        inst.OutputPackagePath = Path.IsPathRooted(outputPath) ? outputPath : Path.Combine(Environment.CurrentDirectory, outputPath);

                        if (inst.ShouldPackage)
                        {
                            inst.DeleteUnsignedPackage = true;
                            inst.UnsignedPackagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pgx");
                        }
                        else
                        {
                            inst.DeleteUnsignedPackage = true;
                            inst.UnsignedPackagePath = inst.OutputPackagePath;
                        }

                        // Determine if verification is required
                        inst.ShouldVerify = p.HasFlag("verify") || p.HasFlag("v") || inst.ShouldSign;

                        if (inst.Validate())
                            _inst = inst;
                    }
                }
                return _inst;
            }
        }

        internal static X509Certificate2 FindCertificate(string certNameOrSerialNumber, bool noUI)
        {
            X509Certificate2Collection certs = null;
            StoreName[] storeNames = (StoreName[])Enum.GetValues(typeof(StoreName));
            StoreLocation[] storeLocations = (StoreLocation[])Enum.GetValues(typeof(StoreLocation));

            // Lookup in all locations
            foreach(var storeLoc in storeLocations)
            {
                // Lookup in all stores
                foreach (var storeName in storeNames)
                {
                    var store = new X509Store(storeName, storeLoc);

                    store.Open(OpenFlags.ReadOnly);
                    // Find the certificate by serial number first
                    certs = store.Certificates.Find(X509FindType.FindBySerialNumber, certNameOrSerialNumber, false)
                                              .Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, false);
                    // Find the certificate by subject name, if we couldn't find one by serial number
                    if (certs == null || certs.Count == 0)
                    {
                        certs = store.Certificates.Find(X509FindType.FindBySubjectName, certNameOrSerialNumber, false)
                                                  .Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, false);
                    }

                    store.Close();
                    if( certs.Count == 1 )
                        return certs[0];
                }
            } 

            throw new Exception("Could not find the certificate requested");
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(InputPath))
            {
                Console.WriteLine("Invalid or missing input path : {0}.", InputPath);
                PrintUsage();
                return false;
            }

            if (string.IsNullOrEmpty(OutputPackagePath))
            {
                Console.WriteLine("Invalid output path : {0}.", OutputPackagePath);
                PrintUsage();
                return false;
            }

            if (ShouldPackage && !ShouldUpdateUrl)
            {
                if (!Directory.Exists(InputPath))
                {
                    Console.WriteLine("Input folder '{0}' does not exist.", InputPath);
                    PrintUsage();
                    return false;
                }
                if (!File.Exists(Path.Combine(InputPath, "manifest.json")))
                {
                    Console.WriteLine("Manifest file (manifest.json) does not exist in the input folder {0}.", InputPath);
                    PrintUsage();
                    return false;
                }
            }

            if(ShouldSign)
            {
                if( Cert == null )
                {
                    Console.WriteLine("Could not locate signing certificate.");
                    PrintUsage();
                    return false;
                }
            }

            if (!ShouldPackage || ShouldSign)
            {
                if (string.IsNullOrEmpty(UnsignedPackagePath))
                {
                    Console.WriteLine("Unsigned package path {0} is invalid.", UnsignedPackagePath);
                    PrintUsage();
                    return false;
                }
                if (!OutputPackagePath.EndsWith(".pgx", StringComparison.InvariantCultureIgnoreCase) || !Directory.Exists(Path.GetDirectoryName(OutputPackagePath)))
                {
                    Console.WriteLine("Output path {0} is invalid.", InputPath);
                    PrintUsage();
                    return false;
                }
            }

            if (ShouldVerify && !ShouldPackage && !OutputPackagePath.EndsWith(".pgx") )
            {
                Console.WriteLine("Can't verify {0}.", OutputPackagePath);
                PrintUsage();
                return false;
            }

            return true;
        }

        void PrintUsage()
        {
            Console.WriteLine("{0} [--v(erify)] [--c(ert)=<name-or-serial-number-of-signing-certificate>] [--i(nput)=<input-folder-path>|<input-package-path>] [--o(utput)=<output-folder-path>|<output-package-path>]",
                                Assembly.GetEntryAssembly().GetName().Name);
        }

        public string InputPath { get; private set; }
        public string UnsignedPackagePath { get; private set; }
        public string OutputPackagePath { get; private set; }
        public Uri PodUrlToUpdate { get; private set; }
        public string PgxPath { get; private set; }
        public X509Certificate2 Cert { get; private set; }
        public bool Verbose { get; private set; }
        public bool ShouldSign { get; private set; }
        public bool ShouldPackage { get; private set; }
        public bool ShouldUpdateUrl { get; private set; }
        public bool ShouldVerify { get; private set; }
        public bool DeleteUnsignedPackage { get; set; }
    }
}
