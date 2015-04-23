using System;
using Microsoft.Practices.Unity;
using Paragon.Plugins;
using Symphony.Mvvm;

namespace Symphony.Shell.HotKeys
{
    public class HotKeyExtension : Extension
    {
        private const string BringToFocusEventName = "bringToFocus";

        private readonly IUnityContainer container;

        public HotKeyExtension(IUnityContainer container)
            : base(container)
        {
            this.container = container;
        }

        protected override void SetupContainer(IUnityContainer container)
        {
            container.RegisterSingleton<IHotKeySettings, HotKeySettings>();
        }

        public override void Initalize(IApplication application)
        {
            base.Initalize(application);

            var applicationWindow = application.FindWindow(PluginExecutionContext.BrowserIdentifier);
            var nativeWindow = new NativeWindow(applicationWindow);

            var service = new HotKeyService(nativeWindow);

            var settings = this
                .container
                .Resolve<IHotKeySettings>();

            this.SetUpHotKeyService(applicationWindow, service, settings);

            this.container
                .Resolve<IEventAggregator>()
                .GetEvent<HotKeyEvents.SaveHotKey>()
                .Subscribe(args => this.OnSaveHotKey(applicationWindow, service, args));
        }

        private void SetUpHotKeyService(
            IApplicationWindow applicationWindow,
            HotKeyService service, 
            IHotKeySettings settings)
        {
            var isEnabled = settings.GetIsHotKeyEnabled();
            var modifiers = settings.GetModifier();
            var keys = settings.GetKeys();

            service.Add(BringToFocusEventName, modifiers, keys, () => this.BringToFocus(applicationWindow));
            if (isEnabled) service.Start();
        }

        private void OnSaveHotKey(
            IApplicationWindow applicationWindow,
            HotKeyService service, 
            HotKeyEvents.SaveGetFocusHotKeyArgs args)
        {
            service.Stop();
            service.Remove(BringToFocusEventName);
            service.Add(BringToFocusEventName, args.Modifiers, args.Keys, () => this.BringToFocus(applicationWindow));
            if (args.IsEnabled) service.Start();
        }

        private void BringToFocus(IApplicationWindow applicationWindow)
        {
            Action showWindowAction = () => applicationWindow.ShowWindow();

            applicationWindow
                .Unwrap()
                .Dispatcher
                .Invoke(showWindowAction);
        }
    }
}
