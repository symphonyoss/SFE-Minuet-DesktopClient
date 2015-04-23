using System.Windows.Forms;
using System.Windows.Input;

namespace Symphony.Shell.HotKeys
{
    public interface IHotKeySettings
    {
        bool GetIsHotKeyEnabled();
        ModifierKeys GetModifier();
        Keys GetKeys();
        void Save(bool isHotKeyEnabled, ModifierKeys modifiers, Keys keys);
    }
}