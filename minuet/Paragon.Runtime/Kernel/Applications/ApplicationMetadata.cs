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
using System.IO.Packaging;
using System.Security;
using Microsoft.Win32;
using Newtonsoft.Json;
using Paragon.Plugins;
using Paragon.Runtime.PackagedApplication;

namespace Paragon.Runtime.Kernel.Applications
{
    /// <summary>
    /// Defines the metadata properties associated with an application.
    /// </summary>
    public class ApplicationMetadata : IApplicationMetadata
    {
        IApplicationPackage _package;
        private ApplicationEnvironment _environment = ApplicationEnvironment.Production;

        public ApplicationMetadata()
        {
            PackageSignatureVerifier = VerifyPackageSignature;
            InstanceId = Guid.NewGuid().ToString();
            WDPort = -1;
        }

        /// <summary>
        /// Application unique id as defined in the gallery. Required.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Application instance ID.
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// Application type as defined in the gallery (one of Packaged or Hosted). Required.
        /// </summary>
        public ApplicationType AppType { get; set; }

        /// <summary>
        /// Id of the workspace this application is a member of. Required.
        /// </summary>
        public string WorkspaceId { get; set; }

        [JsonIgnore]
        public Func<Package,Package> PackageSignatureVerifier{ get; set; }

        /// <summary>
        /// Application environment.
        /// </summary>
        public ApplicationEnvironment Environment 
        {
            get
            {
                return _environment;
            }
            set
            {
                if( value != ApplicationEnvironment.Production )
                {
                    try
                    {
#if ENFORCE_PACKAGE_SECURITY
                        // Only if the current user has developer access, we allow non-production evironment to be set
                        using( RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Policies\\Goldman Sachs\\DashNative\\DevelperAccess") )
                        {
                            if( key.GetValue("Default").ToString().Equals("1", StringComparison.CurrentCultureIgnoreCase) )
                            {
                                _environment = value;
                            }
                        }
#else
                        _environment = value;
#endif
                    }
                    catch
                    {
                        // TODO : Log this
                    }
                }
            }
        }

        /// <summary>
        /// Port to listen on for WebDriver commands. Used in UIAutomationPlugin
        /// </summary>
        public int WDPort { get; set; }

        /// <summary>
        /// Gets the initialization data. If the application a packaged application, this must be a Uri of the package.
        /// If the application is a hosted application, this must be either a Uri of the manifest file or a start url.
        /// </summary>
        public string StartData { get; set; }

        [JsonIgnore]
        public Action<string> UpdateLaunchStatus { get; set; }

        /// <summary>
        /// Standalone apps are not part of the Paragon ecosystem. The Paragon ecosystem
        /// implies an association with Desktop Assistant and extra facilities such
        /// as workspaces.
        /// </summary>
        [JsonIgnore]
        public bool IsStandalone { get; set; }

        public bool Equals(IApplicationMetadata other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Id, other.Id) && string.Equals(InstanceId, other.InstanceId);
        }

        public IApplicationPackage GetApplicationPackage()
        {
            string resolvedUri;
            if (_package == null)
            {
                
                var package = ApplicationPackageResolver.Load(StartData, PackageSignatureVerifier, out resolvedUri);
                if (package == null)
                {
                    throw new Exception("Invalid start data");
                }

                _package = package;
                Id = package.Manifest.Id;
            }
            return _package;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((ApplicationMetadata) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0)*397) ^ (InstanceId != null ? InstanceId.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ApplicationMetadata left, ApplicationMetadata right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ApplicationMetadata left, ApplicationMetadata right)
        {
            return !Equals(left, right);
        }

        Package VerifyPackageSignature( Package inputPackage )
        {
            if( inputPackage.IsSigned() )
            {
                return inputPackage.Verify();
            }
#if ENFORCE_PACKAGE_SECURITY
            else if( Environment == ApplicationEnvironment.Development )
            {
                return inputPackage;
            }
            throw new SecurityException("Unsigned packages are not allowed to run");
#else
            return inputPackage;
#endif
        }
    }
}