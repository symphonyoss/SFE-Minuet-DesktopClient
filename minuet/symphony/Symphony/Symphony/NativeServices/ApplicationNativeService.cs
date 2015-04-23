using System.Windows;
using Newtonsoft.Json.Linq;

namespace Symphony.NativeServices
{
    public class ApplicationNativeService
    {
        public void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ValidationLogger.NullArgument();
                return;
            }

            Logger.Info("[JS] " + message);
        }

        public JObject RefreshAuthCookie()
        {
            JObject json = new JObject();
            json["success"] = true;

            return json;
        }

        public void Shutdown()
        {
            Logger.Info("Shutdown requested");
            
            Application.Current.Shutdown();
        }
    }
}
