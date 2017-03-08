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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using Paragon.Runtime.Kernel.Applications;
using Paragon.Runtime.PackagedApplication;
using System.Threading;

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

                if (!info.ShouldUpdateUrl && info.ShouldPackage && !ParagonAppPackager.Package(info.InputPath, info.UnsignedPackagePath) )
                    throw new Exception("Error in packaging.");

                if (info.ShouldUpdateUrl && !ParagonAppPackager.UpdatePodUrl(info.PodUrlToUpdate, info.PgxPath))
                    throw new Exception("Error in PodUrl update.");

                if (info.ShouldSign)
                {
                    if (!ParagonPackageSigner.Sign(info.UnsignedPackagePath, info.OutputPackagePath, info.Cert))
                        throw new Exception("Error in signing package.");
                }
                else if (info.ShouldPackage && !info.ShouldUpdateUrl)
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