using System;
using Paragon.Plugins;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Timers;
using System.Windows;
using System.Linq;

namespace Symphony.Behaviors
{
    // This class is responsible for pinging back-end to see if upgrade of symphony is required.
    // backend returns json object of type: UpgradeResponse.
    // if shouldUpgrade is false then we do nothing and do not retry until app is started again.
    // if curr app version indicates upgrade not needed then we ping backend again at interval: retryIntervalInSec
    // if upgrade needed then dialog is shown asking user if they want to upgrade
    // ToDo: can user not upgrade if they are below minVersion?
    // if user selects to upgrade then download of msi is initiated and upgrade is started.

    class UpgradeResponse
    {
        public bool shouldUpgrade { get; set; }
        public String minVersion { get; set; }
        public String currentVersion { get; set; }
        public String installURL { get; set; }
        public int retryIntervalInSec { get; set; }
    }

    public class ApplicationUpgradeBehavior
    {
        private IApplication application;
        private HttpWebRequest webRequest;
        private bool fetching = false;
        private Timer checkUpgradeTimer;
        Version version; // current version of the symphony app
        Uri versionReqUrl; // end-point that has upgrade info json
        Window mainWindow;

        public void AttachTo(IApplication application)
        {
            this.application = application;
            this.application.WindowManager.CreatedWindow += this.OnWindowCreated;
        }

        private void OnWindowCreated(IApplicationWindow applicationWindow, bool isMainWindow)
        {
            if (!isMainWindow)
                return;

            this.application.WindowManager.CreatedWindow -= this.OnWindowCreated;

            // ToDo: is there a better way to get main window
            if (applicationWindow is Window)
                mainWindow = applicationWindow as Window;
            else
                return; // can't get window object

            version = new Version(this.application.Package.Manifest.Version);

            Uri baseUri = new Uri(applicationWindow.GetUrl());
            versionReqUrl = new Uri("https://" + baseUri.Host + "/webcontroller/v2/version-requirement");

            fetchUpgradeInfo();
        }

        void fetchUpgradeInfo() 
        {
            if (fetching == true)
                return; // fetch in progress, ignore

            fetching = true;

            // ToDo: make this async

            webRequest = (HttpWebRequest)WebRequest.Create(versionReqUrl);
            webRequest.Method = WebRequestMethods.Http.Get;

            using (var webResponse = webRequest.GetResponse())
            {
                if (((HttpWebResponse)webResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = webResponse.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            var serializer = new JsonSerializer();

                            using (var sr = new StreamReader(stream))
                            {
                                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
                                // only read the part of the JSON resp that we care about
                                if (o["winWrapper"] != null)
                                {
                                    UpgradeResponse res = serializer.Deserialize<UpgradeResponse>(o["winWrapper"].CreateReader());
                                    fetching = false;
                                    processUpgradeReponse(res);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            handleFailure();
        }

        void processUpgradeReponse(UpgradeResponse response)
        {
            if (!response.shouldUpgrade)
            {
                stopTimer();
                return;
            }

            try
            {
                Version minVersion = new Version(response.minVersion);
                Version currVersion = new Version(response.currentVersion);

                if (version >= currVersion && version >= minVersion)
                {
                    // fall safe - don't retry any quicker than 10 mins
                    int retryIntervalInSec = Math.Max(response.retryIntervalInSec, 60 * 10);
                    startTimer(retryIntervalInSec);
                    return;
                }

                Uri downloadUri = new Uri(response.installURL);
                displayUpgradeDialog(downloadUri, minVersion, currVersion);
            }
            catch (Exception) 
            {
                // catch any errors parsing response
                handleFailure();
                return;
            }
        }

        void displayUpgradeDialog(Uri downloadUri, Version minVersion, Version currVersion)
        {
            // ToDo: check against version and display appropriate message in dialog
            System.Windows.Window win = new System.Windows.Window();
            win.Owner = mainWindow;
            bool? dialogResp = win.ShowDialog();

            if (dialogResp == true)
            {
                // ToDo: show upgrade window, download upgrade and if successful then start install
                return;
            }

            // user selected not to upgrade, don't bug them again until app restarts
            stopTimer();
        }

        void stopTimer()
        {
            if (checkUpgradeTimer != null)
            {
                checkUpgradeTimer.Elapsed -= checkUpgradeTimer_Elapsed; 
                checkUpgradeTimer.Stop();
                checkUpgradeTimer.Dispose();
                checkUpgradeTimer = null;
            }
        }

        void handleFailure()
        {
            fetching = false;
            // try again in one hour
            startTimer(3600);
        }

        void startTimer(int internvalInSecs)
        {
            stopTimer();
            checkUpgradeTimer = new Timer(internvalInSecs * 1000.0);
            checkUpgradeTimer.Elapsed += checkUpgradeTimer_Elapsed;
            checkUpgradeTimer.Start();
        }

        void checkUpgradeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            stopTimer();
            // invoke back onto main UI thread, since timer callback is on different thread
            mainWindow.Dispatcher.Invoke(new Action(() =>
            {
                // check again
                fetchUpgradeInfo();
            }));
        }
    }
}