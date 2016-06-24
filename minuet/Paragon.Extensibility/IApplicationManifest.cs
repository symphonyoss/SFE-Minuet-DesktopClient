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
namespace Paragon.Plugins
{
    public interface IApplicationManifest
    {
        string Id { get; set; }

        /// <summary>
        /// Application type as defined in the gallery (one of Packaged or Hosted). Required.
        /// </summary>
        ApplicationType Type { get; set; }

        /// <summary>
        /// Name of the application. Required.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Version of the application. Required.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// Description of the application. Required.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Use the app name as WindowName irrespective of title changes via html code
        /// </summary>
        bool UseAppNameAsWindowTitle { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the app is single instance or not.
        /// </summary>
        bool SingleInstance { get; set; }

        /// <summary>
        /// Indicates the process group name the application belongs to. The name should not have any special characters except spaces. Optional. 
        /// </summary>
        string ProcessGroup { get; set; }
        
        /// <summary>
        /// Application startup information. Required.
        /// </summary>
        IAppInfo App { get; set; }

        /// <summary>
        /// Gets or sets the default window frame type.
        /// </summary>
        FrameType DefaultFrameType { get; set; }

        // Optional
        string[] Permissions { get; set; }

        string[] ExternalUrlWhitelist { get; set; }

        ICORSBypassEntry[] CORSBypassList { get; set; }

        string[] CustomProtocolWhitelist { get; set; }

        bool DisableSpellChecking { get; set; }
        
        /// <summary>
        /// if true passes command line --media-stream-enable to chromium - 
        /// needed to enable screen sharing over webrtc
        /// </summary>
        bool EnableMediaStream { get; set; }

        string BrowserLanguage { get; set; }

        IIconInfo Icons { get; set; }

        INativeServiceInfo[] NativeServices { get; }

        IPluginInfo[] ApplicationPlugins { get; }

        /// <summary>
        /// Gets or sets the custom theme for the application.
        /// </summary>
        string CustomTheme { get; set; }
    }

    public interface INativeServiceInfo
    {
        string Name { get; set; }
        string Executable { get; set; }
    }

    public interface IAppInfo
    {
        /// <summary>
        /// Background script information for a packaged application. Required for packaged applications.
        /// </summary>
        IBackgroundInfo Background { get; set; }

        /// <summary>
        /// Currently unused.
        /// </summary>
        string[] Urls { get; set; }

        /// <summary>
        /// Launch information for a hosted application. Required for hosted applications.
        /// </summary>
        ILaunchInfo Launch { get; set; }

        /// <summary>
        /// Timeout for the creation of the first window of the application. If no window is created within this period, the application will cloase.
        /// Optional. Default value is 10 seconds.
        /// </summary>
        int StartupTimeout { get; set; }
    }

    public interface IIconInfo
    {
        string Icon128 { get; set; }

        string Icon16 { get; set; }
    }

    public interface IPluginInfo
    {
        string Name { get; set; }
        string Path { get; set; }
        string Assembly { get; set; }
        bool RunInRenderer { get; set; }
        List<string> UnmanagedDlls { get; set; }
    }

    public interface ICORSBypassEntry
    {
        string SourceUrl { get; set; }
        string TargetDomain { get; set; }
        string Protocol { get; set; }
        bool AllowTargetSubdomains { get; set; }
    }

    public interface IBackgroundInfo
    {
        string[] Scripts { get; set; }
    }

    public interface ILaunchInfo
    {
        string WebUrl { get; set; }

        string Id { get; set; }
        bool AutoSaveLocation { get; set; }

        string Container { get; set; }

        /// <summary>
        /// Height for the main window. Optional. Default is 750.
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Width for the main window. Optional. Default is 1000.
        /// </summary>
        int Width { get; set; }

        int MinHeight { get; set; }

        int MinWidth { get; set; }

        int MaxHeight { get; set; }

        int MaxWidth { get; set; }

        /// <summary>
        /// Left for the main window. Optional. Default is -1 (no value).
        /// </summary>
        int Left { get; set; }

        /// <summary>
        /// Top for the main window. Optional. Default is -1 (no value).
        /// </summary>
        int Top { get; set; }
    }

    public enum FrameType
    {
        NotSpecified,
        Paragon,
        WindowsDefault,
        None
    }
}