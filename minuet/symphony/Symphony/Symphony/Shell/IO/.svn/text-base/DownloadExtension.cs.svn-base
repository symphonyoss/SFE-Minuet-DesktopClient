using Microsoft.Practices.Unity;
using Paragon.Plugins;

namespace Symphony.Shell.IO
{
    public class DownloadExtension : Extension
    {
        public DownloadExtension(IUnityContainer container)
            : base(container)
        {
        
        }

        public override void Initalize(IApplication application)
        {
            base.Initalize(application);

            //application.Browser.DownloadUpdated += this.OnDownloadUpdated;
        }

        //private void OnDownloadUpdated(object sender, DownloadUpdatedEventArgs args)
        //{
        //    try
        //    {
        //        //if (args.IsComplete)
        //        //{
        //        //    Log.Info("Downloaded file: {0}, size={1} KB", args.FullPath, args.ReceivedBytes / 1024);

        //        //    try
        //        //    {
        //        //        Process.Start("explorer.exe", "/Select, " + args.FullPath);
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        Log.Warn("Unable to open explorer. " + ex.StackTrace);
        //        //    }
        //        //}
        //    }
        //    catch (Exception exception)
        //    {
        //        //Log.Error("Download failed: {0}", exception);
        //    }
        //}
    }
}
