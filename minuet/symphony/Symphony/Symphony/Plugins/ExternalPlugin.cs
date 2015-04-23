using System.Diagnostics;
using Paragon.Plugins;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony.external", IsBrowserSide = true)]
    public class ExternalPlugin
    {
        [JavaScriptPluginMember]
        public void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                ValidationLogger.NullArgument();
                return;
            }

            Process.Start(url);
        }


        [JavaScriptPluginMember]
        public void CallByKerberos(string kerberos)
        {
            if (string.IsNullOrEmpty(kerberos))
            {
                ValidationLogger.NullArgument();
                return;
            }

            Process.Start("da://invoke/call?kerberos=" + kerberos);
        }
    }
}
