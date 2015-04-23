using System;
using System.Threading.Tasks;
using Paragon.Plugins;

namespace Symphony.Shell.WindowManagement
{
    public class WindowInterceptor
    {
        private readonly IWindowPlacementSettings settings;
        //private IWindow window;

        public WindowInterceptor(IWindowPlacementSettings settings)
        {
            this.settings = settings;
        }

        public void AttachToApplication(IApplication application)
        {
            //this.window = application.Window;

            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

            TaskUtilities
                .Delay(50)
                .ContinueWith(() =>
            {
                this.RestoreLocationAndSize();
                //this.AttachTo(this.window);
            }, scheduler);
        }

        private void AttachTo(IApplicationWindow window)
        {
            //window.LocationChanged += (sender, args) => this.SaveLocationAndSize();
            //window.SizeChanged += (sender, args) => this.SaveLocationAndSize();
        }

        private void RestoreLocationAndSize()
        {
            var placementAsXml = this.settings.GetPlacementAsXml();

            if (!string.IsNullOrEmpty(placementAsXml))
            {
                var handle = this.EnsureHandle();
                Win32Api.SetWindowPlacement(handle, placementAsXml);
            }
        }

        private void SaveLocationAndSize()
        {
            var handle = this.EnsureHandle();
            var placementAsXml = Win32Api.GetWindowPlacement(handle);

            this.settings.Save(placementAsXml);
        }

        private IntPtr EnsureHandle()
        {
            return IntPtr.Zero;
            //return this
            //    .window
            //    .NativeWindow
            //    .EnsureHandle();
        }
    }
}
