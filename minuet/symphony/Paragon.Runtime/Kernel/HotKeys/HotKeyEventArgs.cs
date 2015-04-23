using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Paragon.Runtime.Kernel.HotKeys
{
    public class HotKeyEventArgs : EventArgs
    {
        public HotKeyEventArgs(string name, ModifierKeys modifiers, Keys keys)
        {
            Name = name;
            Modifiers = modifiers;
            Keys = keys;
        }

        public string Name { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public Keys Keys { get; set; }
    }
}