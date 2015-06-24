using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Paragon.Plugins;

namespace Paragon.Runtime.Plugins
{
    public sealed class PluginContext : IPluginContext
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private readonly PluginManager _pluginManager;

        private readonly ConcurrentDictionary<string, IV8Plugin> _plugins =
            new ConcurrentDictionary<string, IV8Plugin>();

        private readonly IV8PluginRouter _router;
        private bool _disposed;

        public PluginContext(IV8PluginRouter router, Guid pluginContextId)
        {
            _router = router;
            _pluginManager = new PluginManager(PluginProcess.Renderer);
        }

        public IPluginManager PluginManager
        {
            get { return _pluginManager; }
        }

        public void Initialize(PluginGroup pluginGroup)
        {
            if (pluginGroup == null)
            {
                throw new ArgumentNullException("pluginGroup");
            }

            if (pluginGroup.PluginDescriptors != null)
            {
                foreach (var descriptor in pluginGroup.PluginDescriptors)
                {
                    var current = descriptor;
                    if (!_plugins.TryAdd(current.PluginId, new RemoteV8Plugin(_router, this, current)))
                    {
                        Logger.Warn("Error adding remote plugin with ID: " + current.PluginId);
                    }
                }
            }

            foreach (var plugin in PluginManager.GetAllLocalPlugins())
            {
                var current = plugin;
                if (!_plugins.TryAdd(current.Descriptor.PluginId, new LocalV8Plugin(_router, this, current)))
                {
                    Logger.Warn("Error adding remote plugin with ID: " + current.Descriptor.PluginId);
                }
            }
        }

        public IEnumerable<string> GetPluginIds()
        {
            return _plugins.Keys;
        }

        public IV8Plugin GetPluginById(string pluginId)
        {
            IV8Plugin plugin;
            if (!_plugins.TryGetValue(pluginId, out plugin))
            {
                Logger.Error("Plugin not found: " + pluginId);
            }

            return plugin;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            var plugins = _plugins.Values.ToList();
            _plugins.Clear();
            plugins.ForEach(p => p.Dispose());
        }
    }
}