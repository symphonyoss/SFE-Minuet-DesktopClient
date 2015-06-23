using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Paragon.Plugins;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Plugins
{
    public class PluginManager : IPluginManager
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();

        private static readonly byte[] ParagonKeyToken = typeof (JavaScriptPluginAttribute)
            .Assembly.GetName().GetPublicKeyToken();

        private readonly ConcurrentDictionary<string, Assembly> _loadedAssemblies =
            new ConcurrentDictionary<string, Assembly>();

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

        public void LoadLocalPluginsFromFolder(string folderPath, bool isKernel)
        {
            var directory = new DirectoryInfo(folderPath);
            if (!directory.Exists)
            {
                Logger.Error("Plugin directory not found: " + folderPath);
                return;
            }

            var files = new List<FileInfo>(directory.GetFiles("*.dll"));
            files.AddRange(directory.GetFiles("*.exe"));
            Logger.Info("Inspecting {0} files for plugins", files.Count);
            files.ForEach(f => LoadLocalPluginsFromFile(f.FullName, isKernel));
        }

        private void LoadLocalPluginsFromFile(string pluginAssemblyFilePath, bool isKernel)
        {
            try
            {
                var assembly = VerifyAndLoad(pluginAssemblyFilePath);
                if (assembly == null)
                {
                    Logger.Error("Failed to verify and load local plugin assembly: " + pluginAssemblyFilePath);
                    return;
                }

                foreach (var pluginType in assembly.GetTypes().Where(t =>
                    t.GetCustomAttributes(typeof (JavaScriptPluginAttribute), true).Length > 0))
                {
                    var type = pluginType;
                    var plugin = JavaScriptPlugin.CreateFromType(_managerProcess, type, isKernel);
                    if (plugin == null)
                    {
                        Logger.Warn("Failed to create plugin of type {0} in assembly {1}", type, pluginAssemblyFilePath);
                        continue;
                    }

                    if (plugin.IsDynamic)
                    {
                        Logger.Info("Skipping load of dynamic plugin {0} in assembly {1}", type, pluginAssemblyFilePath);
                        continue;
                    }

                    if (!_localPluginsById.TryAdd(plugin.Descriptor.PluginId, plugin))
                    {
                        Logger.Warn("Failed to add local plugin {0} in assembly {1}", type, pluginAssemblyFilePath);
                    }

                    Logger.Info("Successfully added local plugin {0} from assembly {1}", type, pluginAssemblyFilePath);
                }
            }
            catch (Exception exception)
            {
                Logger.Error("Failed to load plugins from [{0}] because: {1}", pluginAssemblyFilePath, exception);
            }
        }

        private void DynamicPluginDisposed(JavaScriptPlugin dynamicPlugin)
        {
            RemoveLocalPlugin(dynamicPlugin.Descriptor.PluginId);
        }

        private Assembly VerifyAndLoad(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
            {
                throw new ArgumentNullException("assemblyPath");
            }

            Assembly assembly;
            if (_loadedAssemblies.TryGetValue(assemblyPath, out assembly))
            {
                return assembly;
            }

            try
            {
                AssemblyName assemblyName;

                // Verify that the assembly is not tampered with.
                if (!AssemblyIsSignedByParagonAndNotTampered(assemblyPath, out assemblyName))
                {
                    throw new Exception("Could not load plugin, becuase it is either tampered "
                                        + "or it's digital signature cuould not be verified.");
                }

                // Load the assembly into the current AppDomain
                assembly = AppDomain.CurrentDomain.Load(assemblyName);
                if (!_loadedAssemblies.TryAdd(assemblyPath, assembly))
                {
                    Logger.Warn("Attempt to load a duplicate assembly from path: " + assemblyPath);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error loading plugin assembly: " + e);
            }

            return assembly;
        }

        private static bool AssemblyIsSignedByParagonAndNotTampered(string assemblyFilePath, out AssemblyName assemblyName)
        {
            assemblyName = null;

            try
            {
                var verified = false;
                if (!NativeMethods.StrongNameSignatureVerificationEx(assemblyFilePath, true, ref verified))
                {
                    Logger.Warn("Unable to verify strong name for assembly: " + assemblyFilePath);
                    return false;
                }

                assemblyName = AssemblyName.GetAssemblyName(assemblyFilePath);
                var token = assemblyName.GetPublicKeyToken();
                if (!token.SequenceEqual(ParagonKeyToken))
                {
                    Logger.Warn("Invalid strong name found for assembly at: " + assemblyFilePath);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error("Error verifying assembly signature: " + e);
                return false;
            }
        }
    }
}