using System;
using System.Collections.Generic;
using System.Reflection;

namespace Paragon.Runtime.Plugins
{
    public interface IPluginManager
    {
        event Action<JavaScriptPlugin> LocalPluginRemoved;

        bool AddLocalPlugin(object pluginObject);

        object AddApplicationPlugin(string type, Assembly assembly);

        JavaScriptPlugin GetLocalPlugin(string pluginId);

        IEnumerable<JavaScriptPlugin> GetAllLocalPlugins();
    }
}