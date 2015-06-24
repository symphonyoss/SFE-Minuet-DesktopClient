using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using Paragon.Plugins;
using Paragon.Runtime.PackagedApplication;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.Plugins
{
    public class PackagedPluginAssemblyResolver : IDisposable
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private IApplicationPackage _package;
        private readonly string _path;
        
        public PackagedPluginAssemblyResolver(IApplicationPackage package, string pluginPath)
        {
            _path = pluginPath;
            _package = package;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }

        public static List<IParagonPlugin> LoadManagedPlugins(IPluginManager pluginManager, IApplicationPackage package, List<IPluginInfo> plugins)
        {
            if (pluginManager == null)
            {
                throw new ArgumentException("pluginManager");
            }

            if (package == null)
            {
                throw new ArgumentException("package");
            }

            var addedPlugins = new List<IParagonPlugin>();
            if (plugins != null)
            {
                foreach (var appPlugin in plugins)
                {
                    if (!appPlugin.Assembly.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            using (var resolver = new PackagedPluginAssemblyResolver(package, appPlugin.Path))
                            {
                                Logger.Info("Loading managed plugin " + appPlugin.Name);
                                if (appPlugin.UnmanagedDlls != null)
                                {
                                    Logger.Info("Loading the unmanaged DLLs for plugin " + appPlugin.Name);
                                    resolver.LoadUnmanagedDLLs(appPlugin.UnmanagedDlls);
                                }

                                var assembly = Assembly.Load(appPlugin.Assembly);
                                var plugin = pluginManager.AddApplicationPlugin(appPlugin.Name, assembly);
                                if (plugin == null)
                                {
                                    Logger.Info("Could not load plugin " + appPlugin.Name);
                                    continue;
                                }

                                Logger.Info("Loaded managed plugin " + appPlugin.Name);
                                if (plugin is IParagonPlugin)
                                {
                                    addedPlugins.Add((IParagonPlugin) plugin);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Could not create application plugin : " + appPlugin.Name, ex);
                        }
                    }
                }
            }
            return addedPlugins;
        }

        public static Dictionary<string, string> LoadJavaScriptPlugins(IApplicationPackage package, List<IPluginInfo> plugins)
        {
            if (package == null)
            {
                throw new ArgumentException("package");
            }

            var addedPlugins = new Dictionary<string, string>();
            if (plugins != null)
            {
                foreach (var appPlugin in plugins)
                {
                    if (appPlugin.Assembly.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            var pp = package.GetPart(Path.Combine(appPlugin.Path, appPlugin.Assembly));
                            if (pp != null)
                            {
                                using (var ps = pp.GetStream())
                                {
                                    var pv = new StreamReader(ps).ReadToEnd();
                                    if (!string.IsNullOrEmpty(pv))
                                    {
                                        Logger.Info("Resolved render-side JavaScript plugin : " + appPlugin.Name);
                                        addedPlugins[appPlugin.Assembly] = pv;
                                    }
                                    else
                                    {
                                        Logger.Warn("Render-side JavaScript plugin {0} is empty! ", appPlugin.Name);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Could not create application plugin : " + appPlugin.Name, ex);
                        }
                    }
                }
            }
            return addedPlugins;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                // args.Name could be in various formats including a string representation
                // of the full name of the assembly including the strong name key, etc. Use 
                // AssemblyName to parse the string and extract the actual name of the assembly file.
                var assemblyName = new AssemblyName(args.Name);
                var assemblyPath = assemblyName.Name;

                // If a path was supplied in the plugin config, combine it with the assembly name.
                if (!string.IsNullOrEmpty(_path))
                {
                    assemblyPath = Path.Combine(_path, assemblyPath);
                }

                // The assembly name could be the name of the file without the dll or exe
                // extension if the name was parsed from the full assembly name. It could also be
                // the name of the file including the extension if a name in that format was used
                // with Assembly.Load. Try to extract package parts using all scenarios.
                var part = GetPart(assemblyPath)
                            ?? (GetPart(assemblyPath + ".dll")
                                ?? GetPart(assemblyPath + ".exe"));

                if (part == null)
                {
                    // The assembly was not found in the package. This may be ok as the assembly
                    // could be part of the paragon framework in which case it will be loaded further
                    // down the resolve chain.
                    Logger.Warn("Assembly not resolved from app package: " + args.Name);
                    return null;
                }

                byte[] assemblyBytes = ReadPart(part);
                Assembly assembly = null;
                if (assemblyBytes != null && assemblyBytes.Length > 0)
                {
                    // Load the assembly from the byte array.
                    assembly = Assembly.Load(assemblyBytes);

                    // Iterate all references for the resolved assembly.
                    foreach (var name in assembly.GetReferencedAssemblies())
                    {
                        // We need to get the list of loaded assemblies for each iteration 
                        // because the list could change as a result of the Assembly.Load call below.
                        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                        // If the referenced assembly is not yet loaded, attempt to load it.
                        if (loadedAssemblies.All(a => a.FullName != name.FullName))
                        {
                            Assembly.Load(name);
                        }
                    }
                }

                return assembly;
            }
            catch (Exception e)
            {
                Logger.Error("Error loading assembly from package: " + e);
                return null;
            }
        }

        private byte[] ReadPart(PackagePart part)
        {
            byte[] partBytes = null;
            using (var partStream = part.GetStream())
            {

                // If the part stream is already a MemoryStream, cast and use it.
                // Otherwise copy to a new MemoryStream so we can extract the bytes.
                var memStream = partStream as MemoryStream;
                if (memStream != null)
                {
                    partBytes = memStream.ToArray();
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        var buffer = new byte[2048];
                        int read;
                        while ((read = partStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }

                        partBytes = ms.ToArray();
                    }
                }
            }
            return partBytes;
        }

        private IntPtr LoadUnmangedDLL(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }

            var part = GetPart(string.IsNullOrEmpty(_path) ? name : Path.Combine(_path, name));
            if (part == null)
            {
                Logger.Warn("DLL not resolved from app package: " + name);
                return IntPtr.Zero;
            }

            IntPtr dll = IntPtr.Zero;

            try
            {
                byte[] dllBytes = ReadPart(part);
                if (dllBytes != null && dllBytes.Length > 0)
                {
                    // Load the dll.
                    var path = Path.Combine(Path.GetTempPath(), name);
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        fs.Write(dllBytes, 0, dllBytes.Length);
                        fs.Flush();
                    }
                    dll = Win32Api.LoadLibrary(path);
                    if (dll == IntPtr.Zero)
                    {
                        var errCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errCode, string.Format("Error loading unmanged dll {0}", path));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading unmanaged dll: " + ex.Message);
            }

            return dll;
        }

        private IntPtr[] LoadUnmanagedDLLs(List<string> names)
        {
            List<IntPtr> dlls = null;
            if (names != null)
            {
                dlls = new List<IntPtr>();
                foreach (var name in names)
                {
                    dlls.Add(LoadUnmangedDLL(name));
                }
            }
            return dlls != null ? dlls.ToArray() : new IntPtr[0];
        }

        private PackagePart GetPart(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) || _package != null)
                {
                   return _package.GetPart(path);
                }
                Logger.Warn("Package part not found: " + path);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to get package part. Path:{0}, Exception: {1}", path, e.ToString());
            }
            return null;
        }
    }
}
