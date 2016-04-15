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

using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Paragon.Plugins;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Paragon.Runtime.PackagedApplication
{
    public sealed class ApplicationManifest : IApplicationManifest
    {
        /// <summary>
        /// Application startup information. Required.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public AppInfo App { get; set; }

        public IconInfo Icons { get; set; }

        public List<NativeService> NativeServices { get; set; }

        public List<ApplicationPlugin> ApplicationPlugins { get; set; }

        public CORSBypassEntry[] CORSBypassList{ get; set; }
        
        /// <summary>
        /// Id of the application. Required.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// Application type as defined in the gallery (one of Packaged or Hosted). Required.
        /// </summary>
        public ApplicationType Type { get; set; }

        /// <summary>
        /// Name of the application. Required.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Version of the application. Required.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Version { get; set; }

        /// <summary>
        /// Description of the application. Required.
        /// </summary>
        //[JsonProperty(Required = Required.Always)]
        public string Description { get; set; }

        /// <summary>
        /// Use the app name as WindowName irrespective of title changes via the html doc
        /// </summary>
        public bool UseAppNameAsWindowTitle { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the app is single instance or not.
        /// </summary>
        public bool SingleInstance { get; set; }

        /// <summary>
        /// Gets or sets the default window frame type.
        /// </summary>
        public FrameType DefaultFrameType { get; set; }

        IAppInfo IApplicationManifest.App
        {
            get { return App; }
            set
            {
                if (value != null)
                {
                    var app = new AppInfo
                    {
                        StartupTimeout = value.StartupTimeout,
                        Urls = value.Urls
                    };
                    IAppInfo iapp = app;
                    iapp.Launch = value.Launch;
                    iapp.Background = value.Background;
                    App = app;
                }
                else
                {
                    App = null;
                }
            }
        }

        // Optional
        private string _processGroup = string.Empty;

        public string ProcessGroup 
        {
            get
            {
                // By default use the application Id as the ProcessGroup, which means all instances of the same application share a browser process.
                return string.IsNullOrEmpty(_processGroup) ? Id : _processGroup;
            }
            set
            {
                _processGroup = value;
            }
        }

        public string[] Permissions { get; set; }

        public string[] ExternalUrlWhitelist { get; set; }

        ICORSBypassEntry[] IApplicationManifest.CORSBypassList
        {
            get
            {
                return CORSBypassList;
            }
            set
            {
                if (value != null)
                {
                    var list = new List<CORSBypassEntry>();
                    foreach (var e in value)
                    {
                        list.Add(new CORSBypassEntry() { SourceUrl = e.SourceUrl, TargetDomain = e.TargetDomain, Protocol = e.Protocol, AllowTargetSubdomains = e.AllowTargetSubdomains });
                    }
                    CORSBypassList = list.ToArray();
                }
            }
        }

        public string[] CustomProtocolWhitelist { get; set; }

        public bool DisableSpellChecking { get; set; }

        public string BrowserLanguage { get; set; }

        public string CustomTheme { get; set; }

        IIconInfo IApplicationManifest.Icons
        {
            get { return Icons; }
            set { Icons = value != null ? new IconInfo {Icon128 = value.Icon128, Icon16 = value.Icon16} : null; }
        }

        INativeServiceInfo[] IApplicationManifest.NativeServices
        {
            get { return NativeServices != null ? NativeServices.ToArray() : new INativeServiceInfo[0]; }
        }

        IPluginInfo[] IApplicationManifest.ApplicationPlugins
        {
            get { return ApplicationPlugins != null ? ApplicationPlugins.ToArray() : new IPluginInfo[0]; }
        }

        public static ApplicationType GetApplicationType(IAppInfo appInfo)
        {
            var launchInfo = (appInfo != null) ? appInfo.Launch : null;
            var backgroundInfo = (appInfo != null) ? appInfo.Background : null;
            var applicationType = ApplicationType.Packaged;

            if (backgroundInfo != null)
            {
                applicationType = ApplicationType.Packaged;
            }
            else
            {
                if (launchInfo != null)
                {
                    applicationType = ApplicationType.Hosted;
                }
            }

            return applicationType;
        }
    }

    public class IconInfo : IIconInfo
    {
        [JsonProperty("128")]
        public string Icon128 { get; set; }

        [JsonProperty("16")]
        public string Icon16 { get; set; }
    }

    public class CORSBypassEntry : ICORSBypassEntry
    {
        [JsonProperty(Required = Required.Always)]
        public string SourceUrl { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TargetDomain { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Protocol { get; set; }
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling=DefaultValueHandling.Populate)]
        public bool AllowTargetSubdomains { get; set; }
    }

    public class ApplicationPlugin : IPluginInfo
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }
        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Path { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Assembly { get; set; }
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool RunInRenderer { get; set; }
        public List<string> UnmanagedDlls { get; set; }
    }

    public class AppInfo : IAppInfo
    {
        public AppInfo()
        {
            StartupTimeout = 10;
        }

        /// <summary>
        /// Background script information for a packaged application. Required for packaged applications.
        /// </summary>
        public BackgroundInfo Background { get; set; }

        /// <summary>
        /// Launch information for a hosted application. Required for hosted applications. This is used for JsonSerialization.
        /// </summary>
        public LaunchInfo Launch { get; set; }

        IBackgroundInfo IAppInfo.Background
        {
            get { return Background; }
            set { Background = value != null ? new BackgroundInfo {Scripts = value.Scripts} : null; }
        }

        /// <summary>
        /// Currently unused.
        /// </summary>
        public string[] Urls { get; set; }

        ILaunchInfo IAppInfo.Launch
        {
            get { return Launch; }
            set
            {
                if (value != null)
                {
                    Launch = new LaunchInfo
                    {
                        Container = value.Container,
                        Height = value.Height,
                        Width = value.Width,
                        Left = value.Left,
                        Top = value.Top,
                        MaxHeight = value.MaxHeight,
                        MaxWidth = value.MaxWidth,
                        MinHeight = value.MinHeight,
                        MinWidth = value.MinWidth,
                        WebUrl = value.WebUrl
                    };
                }
                else
                {
                    Launch = null;
                }
            }
        }

        /// <summary>
        /// Timeout for the creation of the first window of the application. If no window is created within this period, the application will cloase.
        /// Optional. Default value is 10 seconds.
        /// </summary>
        public int StartupTimeout { get; set; }
    }

    public class BackgroundInfo : IBackgroundInfo
    {
        [JsonProperty(Required = Required.Always)]
        public string[] Scripts { get; set; }
    }

    public class LaunchInfo : ILaunchInfo
    {
        public LaunchInfo()
        {
            Height = 750;
            Width = 1000;
            MinHeight = -1;
            MinWidth = -1;
            MaxHeight = -1;
            MaxWidth = -1;
            Left = -1;
            Top = -1;
            AutoSaveLocation = true;
        }

        /// <summary>
        /// The start url for a hosted application.
        /// </summary>
        [JsonProperty("web_url", Required = Required.Always)]
        public string WebUrl { get; set; }

        /// <summary>
        /// Id (optional) to identify the window. This will  be used to remember the size 
        /// and position of the window and restore that geometry when a window with the same id is later opened.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Default true. Used to say if the window position and geometry if remembered if 
        /// a window with the same id is opened later.
        /// </summary>
        public bool AutoSaveLocation { get; set; }

        public string Container { get; set; }

        /// <summary>
        /// Height for the main window. Optional. Default is 750.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Width for the main window. Optional. Default is 1000.
        /// </summary>
        public int Width { get; set; }

        public int MinHeight { get; set; }

        public int MinWidth { get; set; }

        public int MaxHeight { get; set; }

        public int MaxWidth { get; set; }

        /// <summary>
        /// Left for the main window. Optional. Default is -1 (no value).
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Top for the main window. Optional. Default is -1 (no value).
        /// </summary>
        public int Top { get; set; }
    }

    public class NativeService : INativeServiceInfo
    {
        public string Name { get; set; }
        public string Executable { get; set; }
    }
}