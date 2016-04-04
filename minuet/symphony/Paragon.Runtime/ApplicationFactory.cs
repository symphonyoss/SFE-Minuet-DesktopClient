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
using Paragon.Plugins;
using Paragon.Runtime.Annotations;
using Paragon.Runtime.Kernel.Applications;
using Paragon.Runtime.Kernel.Windowing;

namespace Paragon.Runtime
{
    [UsedImplicitly]
    public class ApplicationFactory
    {
        private readonly Func<ICefWebBrowser> _createBrowser;
        private readonly Func<IApplicationWindowEx> _createWindow;
        private readonly Func<IApplicationWindowManagerEx> _createWindowManager;
        private readonly Func<IParagonPlugin[]> _kernelPlugins;

        public ApplicationFactory(
            Func<IParagonPlugin[]> kernelPlugins, 
            Func<ICefWebBrowser> createBrowser, 
            Func<IApplicationWindowEx> createWindow, 
            Func<IApplicationWindowManagerEx> createWindowManager)
        {
            _createBrowser = createBrowser;
            _createWindow = createWindow;
            _createWindowManager = createWindowManager;
            _kernelPlugins = kernelPlugins;
        }

        public IApplication CreateApplication(IApplicationMetadata metadata, IApplicationPackage package, Dictionary<string, object> args)
        {
			metadata.Id = package.Manifest.Id;
			metadata.AppType = package.Manifest.Type;

			switch (package.Manifest.Type)
            {
                case ApplicationType.Hosted:
                    return CreateWebApplication(metadata, package, args);

                case ApplicationType.Packaged:
                    return CreateWebApplication(metadata, package, args);

                default:
                    throw new Exception("Unknown application type");
            }
        }

        private IApplication CreateWebApplication(IApplicationMetadata metadata, IApplicationPackage package, Dictionary<string, object> args)
        {
            return new WebApplication(
                metadata,
                package.Manifest.App.StartupTimeout,
                package,
                args,
                _kernelPlugins(),
                _createBrowser,
                _createWindow,
                _createWindowManager);
        }
    }
}
