using System;
using System.Windows.Forms;
using System.Windows.Input;
using Symphony.Win32;

namespace Symphony.Shell.HotKeys
{
    public class HotKeyService
    {
        private readonly HotKeyRegistry registry = new HotKeyRegistry();
        private readonly NativeWindow nativeWindow;

        private bool isEnabled;

        public HotKeyService(NativeWindow nativeWindow)
        {
            this.nativeWindow = nativeWindow;
            this.nativeWindow.AddHook(this.WndProc);
        }

        public void Add(string name, ModifierKeys modifiers, Keys keys, Action onExecute)
        {
            if (this.registry.Contains(name))
            {
                throw new ArgumentException("An element with the same key already exists", name);
            }

            var hotKeyCommand = new DelegateHotKeyCommand(modifiers, keys, onExecute);
            var id = hotKeyCommand.GetHashCode();

            this.registry.Register(name, hotKeyCommand);

            NativeMethods.RegisterHotKey(this.nativeWindow.Handle, id, (uint)hotKeyCommand.Modifiers, (uint)hotKeyCommand.Keys);
        }

        public bool Remove(string name)
        {
            IHotKeyCommand command;

            if (this.registry.TryRemove(name, out command))
            {
                NativeMethods.UnregisterHotKey(this.nativeWindow.Handle, command.GetHashCode());
            }

            return false;
        }

        public void Start()
        {
            this.isEnabled = true;
        }

        public void Stop()
        {
            this.isEnabled = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (this.isEnabled && msg == NativeMethods.WM_HOTKEY)
            {
                int modifiers = (lParam.ToInt32() & 0xFFFF);
                int keys = ((lParam.ToInt32() >> 16) & 0xFFFF);
                int id = modifiers ^ keys;

                IHotKeyCommand action;

                if (this.registry.TryGetCommand(id, out action))
                {
                    try
                    {
                        action.Execute();
                        handled = true;
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception.ToString());
                    }
                }
            }

            return IntPtr.Zero;
        }
    }
}
