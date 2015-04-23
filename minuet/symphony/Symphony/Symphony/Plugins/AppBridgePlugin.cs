using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Paragon.Plugins;
using Symphony.NativeServices;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "appbridge", IsBrowserSide = true)]
    public class AppBridgePlugin
    {
        public const string RegisterFileUploadCallbackKey = "FilePlugin.RegisterFileUploadCallback";
        public const string RegisterFocusCallbackKey = "WindowPlugin.RegisterFocusCallback";

        private readonly IUnityContainer container;
        private readonly Dispatcher dispatcher;
        private readonly CallbackLookup callbackLookup;

        private bool isInitialized;

        public AppBridgePlugin()
            : this(new UnityContainer(), Application.Current.Dispatcher)
        {

        }

        public AppBridgePlugin(
            IUnityContainer container,
            Dispatcher dispatcher)
        {
            this.container = container;
            this.dispatcher = dispatcher;

            this.callbackLookup = new CallbackLookup();
        }

        #region Application

        [JavaScriptPluginMember(Name = "Log")]
        public void Log(string message)
        {
            if (!this.isInitialized) return;

            this.container
                .Resolve<ApplicationNativeService>()
                .Log(message);
        }

        [JavaScriptPluginMember(Name = "RefreshAuthCookie")]
        public void RefreshAuthCookie(string callbackFunction)
        {
            // stub contents as the container manages the cookie
        }

        [JavaScriptPluginMember(Name = "Shutdown")]
        public void Shutdown()
        {
            if (!this.isInitialized) return;

            this.container
                .Resolve<ApplicationNativeService>()
                .Shutdown();
        }

        #endregion

        #region Telemetry

        [JavaScriptPluginMember(Name = "GetClientInfoRequest")]
        public void GetClientInfoRequest(string callbackFunction)
        {
            if (!this.isInitialized) return;

            var clientInfo = this
                .container
                .Resolve<TelemetryNativeService>()
                .GetClientInfo();

            //PluginHostContext
            //    .Browser
            //    .ExecuteJavaScript(callbackFunction, clientInfo);
        }

        #endregion

        #region External

        [JavaScriptPluginMember(Name = "OpenUrl")]
        public void OpenUrl(string url)
        {
            if (!this.isInitialized) return;

            this.container
                .Resolve<ExternalNativeService>()
                .OpenUrl(url);
        }


        [JavaScriptPluginMember(Name = "CallByKerberos")]
        public void CallByKerberos(string kerberos)
        {
            if (!this.isInitialized) return;

            this.container
                .Resolve<ExternalNativeService>()
                .CallByKerberos(kerberos);
        }

        #endregion

        #region File

        [JavaScriptPluginMember(Name = "RegisterFileUploadCallback")]
        public void RegisterFileUploadCallback(string callback)
        {
            this.callbackLookup
                .Register(RegisterFileUploadCallbackKey, callback);
        }

        [JavaScriptPluginMember(Name = "OpenScreenSnippetTool")]
        public void OpenScreenSnippetTool()
        {
            
        }

        #endregion

        #region Notifications

        [JavaScriptPluginMember(Name = "ClearAlerts")]
        public void ClearAlerts()
        {
            
        }

        [JavaScriptPluginMember(Name = "PlayChime")]
        public void PlayChime()
        {
            
        }

        [JavaScriptPluginMember(Name = "PostAlert")]
        public void PostAlert(string json)
        {
            
        }

        [JavaScriptPluginMember(Name = "ShowAlertSettings")]
        public void ShowAlertSettings()
        {
            
        }

        [JavaScriptPluginMember(Name = "RemoveAlertGrouping")]
        public void RemoveAlertGrouping(string group)
        {
            
        }

        #endregion

        #region Cache

        [JavaScriptPluginMember(Name = "GetTempValue")]
        public void GetTempValue(string key, string callback)
        {
            if (!this.isInitialized) return;

            var result = this.container
                .Resolve<CacheNativeService>()
                .GetValue(key);

            //PluginHostContext
            //    .Browser
            //    .ExecuteJavaScript(callback, result);
        }

        [JavaScriptPluginMember(Name = "SetTempValue")]
        public void SetTempValue(string key, string value)
        {
            if (!this.isInitialized) return;

            this.container
                .Resolve<CacheNativeService>()
                .SetValue(key, value);
        }

        #endregion

        #region Window

        [JavaScriptPluginMember(Name = "Activate")]
        public void Activate(string windowName, bool activate)
        {
            if (!this.isInitialized) return;

            Action activateAction = () => this
                .container
                .Resolve<WindowNativeService>()
                .Activate(windowName, activate);

            this.dispatcher
                .Invoke(activateAction);
        }

        [JavaScriptPluginMember(Name = "Close")]
        public void Close(string windowName)
        {
            if (!this.isInitialized) return;

            Action closeWindowAction = () => this
                .container
                .Resolve<WindowNativeService>()
                .Close(windowName);

            this.dispatcher
                .Invoke(closeWindowAction);
        }

        [JavaScriptPluginMember(Name = "GetActiveWindow")]
        public void GetActiveWindow(string callbackFunction)
        {
            if (!this.isInitialized) return;

            //var browser = PluginHostContext.Browser;

            Action getActiveWindowAction = () =>
            {
                var json = this
                    .container
                    .Resolve<WindowNativeService>()
                    .GetActiveWindow();

                if (json != null)
                {
                    //browser.ExecuteJavaScript(callbackFunction, json);
                }
            };

            this.dispatcher
                .Invoke(getActiveWindowAction);
        }

        [JavaScriptPluginMember(Name = "RegisterFocusCallback")]
        public void RegisterFocusCallback(string callbackFunction)
        {
            this.callbackLookup
                .Register(RegisterFocusCallbackKey, callbackFunction);
        }

        [JavaScriptPluginMember(Name = "SetMinWidth")]
        public void SetMinWidth(string windowName, int minWidth)
        {
            if (!this.isInitialized) return;

            Action setMinWidthAction = () => this
                .container
                .Resolve<WindowNativeService>()
                .SetMinWidth(windowName, minWidth);

            this.dispatcher
                .Invoke(setMinWidthAction);
        }

        #endregion
    }
}
