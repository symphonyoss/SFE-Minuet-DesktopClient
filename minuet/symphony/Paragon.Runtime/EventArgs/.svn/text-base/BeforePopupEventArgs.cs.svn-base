using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class BeforePopupEventArgs : EventArgs
    {
        public BeforePopupEventArgs(
            string targetUrl,
            string targetFrameName,
            CefPopupFeatures popupFeatures,
            CefWindowInfo windowInfo,
            CefClient client,
            bool noJavascriptAccess)
        {
            TargetUrl = targetUrl;
            TargetFrameName = targetFrameName;
            PopupFeatures = popupFeatures;
            WindowInfo = windowInfo;
            Client = client;
            NoJavascriptAccess = noJavascriptAccess;
            NeedCustomPopupWindow = false;
            Cancel = false;
        }

        /// <summary>
        /// When true, the popup will be created in a new render process and the javascript will be suppressed. 
        /// </summary>
        public bool NoJavascriptAccess { get; set; }

        /// <summary>
        /// The client for the new popup. Handler should use extreme caution in changing the property.
        /// </summary>
        public CefClient Client { get; set; }

        /// <summary>
        /// Setting for the new popup. Handler can change the window information.
        /// </summary>
        public CefWindowInfo WindowInfo { get; private set; }

        /// <summary>
        /// The requested features of the new popup.
        /// </summary>
        public CefPopupFeatures PopupFeatures { get; private set; }

        /// <summary>
        /// Name of the target frame. This may be empty.
        /// </summary>
        public string TargetFrameName { get; private set; }

        /// <summary>
        /// The target url of the new popup.
        /// </summary>
        public string TargetUrl { get; private set; }

        /// <summary>
        /// When true, indicates that the handler would like to show a custom window. 
        /// This property is honored only when Cancel is set to false.
        /// </summary>
        public bool NeedCustomPopupWindow { get; set; }

        /// <summary>
        /// If false and NeedCustomPopupWindow is true, ShowPopup will be fired.
        /// If false and NeedCustomPopupWindow is false, a system created standard popup window will be used.
        /// If true the NeedCustomPopupWindow is ignored and the popup will be suppressed.
        /// </summary>
        public bool Cancel { get; set; }
    }
}