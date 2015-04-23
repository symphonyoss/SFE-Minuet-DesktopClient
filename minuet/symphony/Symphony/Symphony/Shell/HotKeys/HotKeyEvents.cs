using System.Windows.Forms;
using System.Windows.Input;
using Symphony.Mvvm;

namespace Symphony.Shell.HotKeys
{
    public class HotKeyEvents
    {
        public class SaveHotKey : PubSubEvent<SaveGetFocusHotKeyArgs>
        {
            
        }

        public class SaveGetFocusHotKeyArgs
        {
            public bool IsEnabled { get; set; }
            public ModifierKeys Modifiers { get; set; }
            public Keys Keys { get; set; }
        }
    }
}
