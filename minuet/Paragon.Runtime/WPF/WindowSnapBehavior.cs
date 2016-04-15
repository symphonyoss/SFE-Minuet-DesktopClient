using System;
using System.Windows.Interactivity;
using System.Windows.Interop;

namespace Paragon.Runtime.WPF
{
    internal sealed class WindowSnapBehavior : Behavior<ParagonWindow>, IDisposable
    {
        private WindowSnapManager _snapManager;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject.IsLoaded)
            {
                _snapManager = new WindowSnapManager(new WindowInteropHelper(AssociatedObject).Handle);
            }
            else
            {
                AssociatedObject.SourceInitialized += AssociatedObjectOnSourceInitialized;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            _snapManager.Dispose();
        }

        private void AssociatedObjectOnSourceInitialized(object sender, EventArgs e)
        {
            AssociatedObject.SourceInitialized -= AssociatedObjectOnSourceInitialized;
            _snapManager = new WindowSnapManager(new WindowInteropHelper(AssociatedObject).Handle);
        }

        public void Dispose()
        {
            _snapManager.Dispose();
        }
    }
}