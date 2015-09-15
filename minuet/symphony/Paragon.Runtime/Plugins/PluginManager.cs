using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Paragon.Plugins;

namespace Paragon.Runtime.Plugins
{
    public class PluginManager : IPluginManager
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();

        private readonly ConcurrentDictionary<string, JavaScriptPlugin> _localPluginsById =
            new ConcurrentDictionary<string, JavaScriptPlugin>();

        /// <summary>
        /// The process this plugin manager resides in.
        /// </summary>
        private readonly PluginProcess _managerProcess;

        public PluginManager(PluginProcess managerProcess)
        {
            _managerProcess = managerProcess;
        }

        public event Action<JavaScriptPlugin> LocalPluginRemoved;

        public bool AddLocalPlugin(object pluginObject)
        {
            var plugin = pluginObject as JavaScriptPlugin;
            if (plugin == null)
            {
                plugin = JavaScriptPlugin.CreateFromObject(_managerProcess, pluginObject);
                if (plugin == null)
                {
                    Logger.Error("Failed to wrap unknown plugin type");
                    return false;
                }
            }

            if (!_localPluginsById.TryAdd(plugin.Descriptor.PluginId, plugin))
            {
                Logger.Warn("Attempt to add a duplicate plugin");
                return false;
            }

            if (plugin.IsDynamic)
            {
                plugin.DynamicPluginDisposed += DynamicPluginDisposed;
            }

            Logger.Info("Local plugin added successfully: " + plugin.Descriptor.PluginId);
            return true;
        }

        public object AddApplicationPlugin(string type, Assembly assembly)
        {
            try
            {
                var t = assembly.GetType(type);
                if (t.GetCustomAttributes(typeof (JavaScriptPluginAttribute), true).Length == 0)
                {
                    Logger.Error("Plugin attribute not found on type: " + type);
                    return null;
                }

                var plugin = JavaScriptPlugin.CreateFromType(_managerProcess, t, false);
                if (plugin == null)
                {
                    Logger.Error("Failed to create plugin instance for type: " + type);
                    return null;
                }

                if (plugin.IsDynamic)
                {
                    Logger.Error("Dynamic application plugins are not allowed - type: " + type);
                    return null;
                }

                if (!_localPluginsById.TryAdd(plugin.Descriptor.PluginId, plugin))
                {
                    Logger.Error("Failed to add application plugin for type: " + type);
                    return null;
                }

                Logger.Info("Application plugin added successfully: " + plugin.Descriptor.PluginId);

                return plugin.NativeObject;
            }
            catch (Exception e)
            {
                Logger.Error("Error adding application plugin: " + e);
                return null;
            }
        }

        public JavaScriptPlugin GetLocalPlugin(string pluginId)
        {
            JavaScriptPlugin plugin;
            if (!_localPluginsById.TryGetValue(pluginId, out plugin))
            {
                Logger.Error("Requested plugin not found: " + pluginId);
                return null;
            }

            return plugin;
        }

        public IEnumerable<JavaScriptPlugin> GetAllLocalPlugins()
        {
            return _localPluginsById.Values.ToArray();
        }

        public void RemoveLocalPlugin(string pluginId)
        {
            JavaScriptPlugin plugin;
            if (!_localPluginsById.TryRemove(pluginId, out plugin))
            {
                Logger.Warn("Failed to remove plugin - no plugin found with the specified ID: " + pluginId);
                return;
            }

            if (plugin != null && plugin.IsDynamic)
            {
                plugin.DynamicPluginDisposed -= DynamicPluginDisposed;
            }

            var localPluginRemoved = LocalPluginRemoved;
            if (localPluginRemoved != null)
            {
                localPluginRemoved(plugin);
            }

            Logger.Info("Successfully removed plugin: " + pluginId);
        }

        private void DynamicPluginDisposed(JavaScriptPlugin dynamicPlugin)
        {
            RemoveLocalPlugin(dynamicPlugin.Descriptor.PluginId);
        }
    }
}