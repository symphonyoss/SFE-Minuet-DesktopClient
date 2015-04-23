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
