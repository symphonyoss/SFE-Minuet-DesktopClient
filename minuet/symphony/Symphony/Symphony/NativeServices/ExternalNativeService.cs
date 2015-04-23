using System.Diagnostics;

namespace Symphony.NativeServices
{
    public class ExternalNativeService
    {
        public void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                ValidationLogger.NullArgument();
                return;
            }

            Process.Start(url);
        }

        public void CallByKerberos(string kerb)
        {
            if (string.IsNullOrEmpty(kerb))
            {
                ValidationLogger.NullArgument();
                return;
            }

            Process.Start("da://invoke/call?kerberos=" + kerb);
        }
    }
}
