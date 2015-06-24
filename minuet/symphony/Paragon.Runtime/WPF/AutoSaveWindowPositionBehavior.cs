using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Interactivity;
using System.Windows.Interop;
using Paragon.Runtime.Kernel.Windowing;
using Paragon.Runtime.Win32;

namespace Paragon.Runtime.WPF
{
    internal class AutoSaveWindowPositionBehavior : Behavior<ApplicationWindow>
    {
        private const string WindowPosDir = "ParagonWinPos";
        private IntPtr? _hwnd;
        private IsolatedStorageFile _store;

        protected override void OnAttached()
        {
            base.OnAttached();

            _store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            _store.CreateDirectory(WindowPosDir);

            AssociatedObject.LocationChanged += OnSizeOrLocationChanged;

            if (AssociatedObject.IsInitialized)
            {
                Init();
            }
            else
            {
                AssociatedObject.SourceInitialized += (s, e) => Init();
            }
        }

        private void OnSizeOrLocationChanged(object sender, EventArgs e)
        {
            if (!_hwnd.HasValue)
            {
                return;
            }

            var placement = default(WINDOWPLACEMENT);
            if (NativeMethods.GetWindowPlacement(_hwnd.Value, ref placement))
            {
                Save(AssociatedObject.GetId() + AssociatedObject.GetAppId(), placement);
            }
        }

        private void Init()
        {
            _hwnd = new WindowInteropHelper(AssociatedObject).Handle;

            WINDOWPLACEMENT placement;
            if (TryGetPosition(AssociatedObject.GetId()+AssociatedObject.GetAppId(), out placement))
            {
                NativeMethods.SetWindowPlacement(_hwnd.Value, ref placement);
            }
        }

        private bool TryGetPosition(string appAndWindowId, out WINDOWPLACEMENT placement)
        {
            var file = Path.Combine(WindowPosDir, appAndWindowId);
            placement = default(WINDOWPLACEMENT);
            if (_store.GetFileNames(file).Length == 0)
            {
                return false;
            }

            using (var reader = new StreamReader(new IsolatedStorageFileStream(file, FileMode.OpenOrCreate, _store)))
            {
                var str = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(str))
                {
                    placement = StructConverter.FromString<WINDOWPLACEMENT>(str);
                    return true;
                }

                return false;
            }
        }

        private void Save(string appAndWindowId, WINDOWPLACEMENT placement)
        {
            var file = Path.Combine(WindowPosDir, appAndWindowId);
            using (var writer = new StreamWriter(new IsolatedStorageFileStream(file, FileMode.Create, FileAccess.Write, _store)))
            {
                writer.Write(StructConverter.ToString(placement));
                writer.Flush();
            }
        }
    }
}