using System;
using System.Collections.Generic;

namespace Paragon.Runtime.Plugins
{
    public interface IPluginContext : IDisposable
    {
        IPluginManager PluginManager { get; }
        IEnumerable<string> GetPluginIds();
        IV8Plugin GetPluginById(string pluginId);
    }
}