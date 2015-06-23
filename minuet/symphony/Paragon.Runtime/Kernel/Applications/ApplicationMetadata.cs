using System;
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
        public ApplicationMetadata()
        {
            InstanceId = Guid.NewGuid().ToString();
            Environment = ApplicationEnvironment.Production;
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

        /// <summary>
        /// Application environment.
        /// </summary>
        public ApplicationEnvironment Environment { get; set; }

        public string ApplicationFamily { get; set; }

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
                var package = ApplicationPackageResolver.Load(StartData, out resolvedUri);
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
    }
}