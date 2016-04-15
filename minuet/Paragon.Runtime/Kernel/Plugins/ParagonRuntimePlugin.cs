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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using Newtonsoft.Json;
using Paragon.Plugins;
using Paragon.Runtime.Annotations;
using Paragon.Runtime.Desktop;
using Paragon.Runtime.Kernel.Applications;
using Paragon.Runtime.PackagedApplication;

namespace Paragon.Runtime.Kernel.Plugins
{
    [JavaScriptPlugin(Name = "paragon.runtime")]
    public class ParagonRuntimePlugin : ParagonPlugin
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly IEnumerable<INativeServiceInfo> _nativeServices;

        public ParagonRuntimePlugin(IEnumerable<INativeServiceInfo> nativeServices)
        {
            _nativeServices = nativeServices;
        }

        [JavaScriptPluginMember(Name = "onAppExited"), UsedImplicitly]
        public event JavaScriptPluginCallback AppExited;

        [JavaScriptPluginMember(Name = "onAppLaunched"), UsedImplicitly]
        public event JavaScriptPluginCallback AppLaunched;

        [JavaScriptPluginMember, UsedImplicitly]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public NativePort ExecService(string application)
        {
            Logger.Debug(string.Format("Connecting to native service {0}", application));
            var service = _nativeServices.First(ns => application.Equals(ns.Name));
            if (service != null)
            {
                var executable = service.Executable;
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    FileName = executable,
                    WorkingDirectory = Path.GetDirectoryName(executable),
                    UseShellExecute = false
                };

                var p = Process.Start(startInfo);
                return new NativePort(p);
            }
            throw new Exception("Unknown native service!");
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public IParagonAppInfo[] GetRunningApps()
        {
            return ParagonDesktop.GetAllApps().ToArray();
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public IParagonAppInfo GetAppByInstanceId(string instanceId)
        {
            return ParagonDesktop.FindApps(opts => opts.InstanceId = instanceId).FirstOrDefault();
        }

        [JavaScriptPluginMember, UsedImplicitly]
        public string GetVersion()
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly != null)
            {
                var assemblyName = assembly.GetName();

                if (assemblyName.Version != null)
                {
                    return assemblyName.Version.ToString();
                }
            }

            return null;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool LaunchApp(string packageUri, string instanceId)
        {
            Logger.Debug("Launching application...");

            string resolvedPackageUri;
            var package = ApplicationPackageResolver.Load(packageUri, (p) => p, out resolvedPackageUri);
            if (package == null)
            {
                throw new Exception("Could not resolve pacakge from " + packageUri);
            }

            var appId = package.Manifest.Id;
            var appType = package.Manifest.Type;

            Logger.Info(string.Format("Launching {0} application {1} ...", appType, appId));

            if (instanceId == null)
            {
                instanceId = Guid.NewGuid().ToString();
            }

            var environment = Application.Metadata.Environment;
            var exePath = Assembly.GetEntryAssembly().Location;
            var isStandalone = Application.Metadata.IsStandalone;
            var args = ParagonCommandLineParser.CreateCommandLine(environment, resolvedPackageUri, appType, appId, instanceId, isStandalone);
            var process = Process.Start(new ProcessStartInfo(exePath, args) {ErrorDialog = false});
            return process != null;
        }

        [JavaScriptPluginMember, UsedImplicitly]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool CloseApp(string instanceId)
        {
            var app = ParagonDesktop.GetApp(instanceId);
            if (app == null)
            {
                return false;
            }

            var proc = Process.GetProcessById(app.BrowserPid);
            if (proc.HasExited)
            {
                return true;
            }

            proc.EnableRaisingEvents = true;
            proc.CloseMainWindow();
            proc.WaitForExit(2000);
            return proc.HasExited;
        }

        protected override void OnInitialize()
        {
            Logger.Debug("OnInitialize");
            base.OnInitialize();
            AppEvents.AppExited += OnAppExited;
            AppEvents.AppLaunched += OnAppLaunched;
        }

        protected override void OnShutdown()
        {
            Logger.Debug("OnShutdown");
            AppEvents.AppExited -= OnAppExited;
            AppEvents.AppLaunched -= OnAppLaunched;
        }

        private void OnAppLaunched(object sender, AppEventArgs e)
        {
            AppLaunched.Raise(() => new object[] {e.AppInfo});
        }

        private void OnAppExited(object sender, AppEventArgs e)
        {
            AppExited.Raise(() => new object[] {e.AppInfo});
        }

        [JavaScriptPlugin]
        public class NativePort
        {
            private static readonly ILogger Logger = ParagonLogManager.GetLogger();
            private readonly Process _process;

            public NativePort(Process process)
            {
                _process = process;
                new Action(ReadMessages).BeginInvoke(null, null);
                new Action(WaitForExit).BeginInvoke(null, null);
            }

            [JavaScriptPluginMember, UsedImplicitly]
            public event JavaScriptPluginCallback OnMessage;

            [JavaScriptPluginMember, UsedImplicitly]
            public event JavaScriptPluginCallback OnDisconnect;

            [JavaScriptPluginMember, UsedImplicitly]
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public void PostMessage(object message)
            {
                var msg = JsonConvert.SerializeObject(message);
                var msgBuffer = Encoding.UTF8.GetBytes(msg);
                var lenBuffer = BitConverter.GetBytes(msgBuffer.Length);
                var writer = _process.StandardInput;
                writer.BaseStream.Write(lenBuffer, 0, lenBuffer.Length);
                writer.BaseStream.Write(msgBuffer, 0, msgBuffer.Length);
                writer.BaseStream.Flush();
                Logger.Info("Native Connect Port : sending request (OnMessage) to service : {0}", msg);
            }

            private void ReadMessages()
            {
                using (var reader = _process.StandardOutput)
                {
                    while (true)
                    {
                        var lenBuffer = new byte[4];
                        if ((reader.BaseStream.Read(lenBuffer, 0, lenBuffer.Length)) <= 0)
                        {
                            continue;
                        }

                        var msgLen = BitConverter.ToInt32(lenBuffer, 0);
                        if (msgLen <= 0)
                        {
                            continue;
                        }

                        var msgBuffer = new byte[msgLen];
                        if ((reader.BaseStream.Read(msgBuffer, 0, msgBuffer.Length)) <= 0)
                        {
                            continue;
                        }

                        var msg = Encoding.UTF8.GetString(msgBuffer);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            FireOnMessage(msg);
                        }
                    }
                }
            }

            private void FireOnMessage(string line)
            {
                var args = JsonConvert.DeserializeObject(line);
                if (OnMessage != null && args != null)
                {
                    Logger.Info("Native Connect Port : sending response (OnMessage) to client : {0}", line);
                    OnMessage(args);
                }
            }

            private void WaitForExit()
            {
                _process.WaitForExit();
                if (OnDisconnect != null)
                {
                    OnDisconnect();
                }
            }
        }
    }
}