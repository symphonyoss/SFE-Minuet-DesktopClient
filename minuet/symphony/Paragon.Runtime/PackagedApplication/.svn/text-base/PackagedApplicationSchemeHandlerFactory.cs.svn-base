using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Paragon.Plugins;
using Xilium.CefGlue;

namespace Paragon.Runtime.PackagedApplication
{
    /// <summary>
    /// Factory that creates resource handlers for a packaged application.
    /// </summary>
    public sealed class PackagedApplicationSchemeHandlerFactory : CefSchemeHandlerFactory
    {
        private const string EventPage = "_events.html";
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        private static readonly string Domain;
        private static readonly string SchemeName;
        private readonly IApplicationPackage _package;
        private string _baseUrl;

        static PackagedApplicationSchemeHandlerFactory()
        {
            var hostNameParts = Dns.GetHostEntry("localhost").HostName.Split('.');
            Domain = hostNameParts.Length >= 2 ? hostNameParts[hostNameParts.Length - 2] + "." + hostNameParts[hostNameParts.Length - 1] : hostNameParts[0];
            SchemeName = "http";
            ErrorPage = SchemeName + "://error" + "." + Domain;
        }

        public PackagedApplicationSchemeHandlerFactory(IApplicationPackage package)
        {
            _package = package;
            Register();
        }

        public static string ErrorPage { get; private set; }

        public string EventPageUrl
        {
            get { return _baseUrl + "/" + EventPage; }
        }

        protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
        {
            if (!string.IsNullOrEmpty(schemeName) && schemeName.Equals(SchemeName, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    var absolutePath = new Uri(request.Url).AbsolutePath;
                    if (!string.IsNullOrEmpty(absolutePath) && absolutePath.EndsWith(EventPage))
                    {
                        var scripts = GetApplicationBackgroundScripts();
                        if (scripts.Length != 0)
                        {
                            var memoryStream = new MemoryStream();
                            var writer = new StreamWriter(memoryStream, Encoding.UTF8, 2048);
                            {
                                writer.Write(@"<!DOCTYPE html>
                                    <html lang=""en"">
                                       <head/>
                                       <body>");
                                foreach (var script in scripts)
                                {
                                    writer.Write("<script src=\"{0}\"></script>\r\n", script);
                                }
                                writer.Write(@"   </body>
                                    </html>");
                                writer.Flush();
                            }
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            return new StreamResourceHandler("text/html", memoryStream);
                        }
                    }

                    var resourcePart = _package.GetPart(absolutePath);
                    if (resourcePart != null)
                    {
                        return new StreamResourceHandler(resourcePart.ContentType, resourcePart.GetStream());
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(fmt => fmt("Failed to create resource handler for [{0}] because: {1}", request.Url, exception));
                }
            }
            return null;
        }

        private string[] GetApplicationBackgroundScripts()
        {
            return _package.Manifest != null &&
                   _package.Manifest.App != null &&
                   _package.Manifest.App.Background != null &&
                   _package.Manifest.App.Background.Scripts != null &&
                   _package.Manifest.App.Background.Scripts.Length != 0
                ? _package.Manifest.App.Background.Scripts
                : new string[0];
        }

        private void Register()
        {
            var hostName = new Regex("[^a-zA-Z0-9]").Replace(_package.Manifest.Name, "") + "." + Domain;
            var baseUrl = SchemeName + "://" + hostName;
            baseUrl = baseUrl.ToLower();
            CefRuntime.AddCrossOriginWhitelistEntry(baseUrl, "http", Domain, true);
            CefRuntime.AddCrossOriginWhitelistEntry(baseUrl, "https", Domain, true);

            if (!CefRuntime.RegisterSchemeHandlerFactory(SchemeName, hostName.ToLower(), this))
            {
                Logger.Error(string.Format("Packaged application {0}: could not register scheme handler factory", _package.Manifest.Name));
                return;
            }
            _baseUrl = baseUrl;
        }
    }
}