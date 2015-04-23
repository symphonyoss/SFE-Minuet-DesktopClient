//
// Simplified version of code taken from a Microsoft blog here: http://blogs.microsoft.co.il/blogs/arik/SingleInstance.cs.txt
//

using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Paragon.Runtime.Win32
{
    /// <summary>
    /// Implemented by <see cref="System.Windows.Application"/> instances to provide access to
    /// command line arguments when a second instance of a single instance app is started.
    /// </summary>
    public interface ISingleInstanceApp
    {
        /// <summary>
        /// Called when a second instance of a single instance app is started.
        /// </summary>
        /// <param name="args">Command line arguments that were provided to the second instance.</param>
        void SignalExternalCommandLineArgs(string args);
    }

    /// <summary>
    /// Utility class used to enforce single instance app behavior.
    /// </summary>
    public static class SingleInstance
    {
        private static Mutex _mutex;
        private static IpcServerChannel _channel;
        private static string _commandLineArgs;

        /// <summary>
        /// Initialize an app as a single instance.
        /// </summary>
        /// <param name="uniqueName">A unique name that will be used to identify this instance.</param>
        /// <returns>Returns true if the current instance is the first one in, otherwise false.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool InitializeAsFirstInstance(string uniqueName)
        {
            // Attempt to create a global mutex. If an instance already exists with the same 
            // name, send it the command line args that were provided to this instance.
            _commandLineArgs = Environment.CommandLine;
            var mutexName = uniqueName + Environment.UserName;
            var channelName = string.Concat(mutexName, ":SingleInstanceIpcChannel");
            bool isFirstInstance;
            _mutex = new Mutex(true, mutexName, out isFirstInstance);

            if (isFirstInstance)
            {
                CreateRemoteService(channelName);
            }
            else
            {
                SignalFirstInstance(channelName, _commandLineArgs);
            }

            return isFirstInstance;
        }

        /// <summary>
        /// Cleanup single instance related resources.
        /// </summary>
        public static void Cleanup()
        {
            if (_mutex != null)
            {
                _mutex.Close();
                _mutex = null;
            }

            if (_channel != null)
            {
                ChannelServices.UnregisterChannel(_channel);
                _channel = null;
            }
        }

        private static void CreateRemoteService(string channelName)
        {
            // Create a remoting service used to receive command line args from
            // any new instances that are started while this instance is running.
            var serverProvider = new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full};
            var props = new Dictionary<string, string> {{"name", channelName}, {"portName", channelName}, {"exclusiveAddressUse", "false"}};
            _channel = new IpcServerChannel(props, serverProvider);
            ChannelServices.RegisterChannel(_channel, true);
            var remoteService = new SingleInstanceService();
            RemotingServices.Marshal(remoteService, typeof (SingleInstanceService).Name);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private static void SignalFirstInstance(string channelName, string args)
        {
            // Send command line args from the new instance to the existing one via remoting.
            var channel = new IpcClientChannel();
            ChannelServices.RegisterChannel(channel, true);
            var serviceUrl = string.Concat("ipc://", channelName, "/", typeof (SingleInstanceService).Name);

            var firstInstance = (SingleInstanceService) RemotingServices.Connect(
                typeof (SingleInstanceService), serviceUrl);

            if (firstInstance != null)
            {
                firstInstance.InvokeFirstInstance(args);
            }
        }

        private class SingleInstanceService : MarshalByRefObject
        {
            public void InvokeFirstInstance(string args)
            {
                if (Application.Current == null)
                {
                    return;
                }

                var app = Application.Current as ISingleInstanceApp;
                if (app != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() => app.SignalExternalCommandLineArgs(args)));
                }
            }

            public override object InitializeLifetimeService()
            {
                return null;
            }
        }
    }
}