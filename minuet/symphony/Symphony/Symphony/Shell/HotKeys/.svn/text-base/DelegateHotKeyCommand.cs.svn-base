using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Symphony.Shell.HotKeys
{
    public class DelegateHotKeyCommand : IHotKeyCommand
    {
        private readonly Action onExecute;

        public DelegateHotKeyCommand(ModifierKeys modifiers, Keys keys, Action onExecute)
        {
            this.Modifiers = modifiers;
            this.Keys = keys;
            this.onExecute = onExecute;
        }

        public ModifierKeys Modifiers { get; private set; }
        public Keys Keys { get; private set; }

        public void Execute()
        {
            if (this.onExecute != null) this.onExecute();
        }

        public override int GetHashCode()
        {
            return (int)this.Modifiers ^ (int)this.Keys;
        }
    }
}
