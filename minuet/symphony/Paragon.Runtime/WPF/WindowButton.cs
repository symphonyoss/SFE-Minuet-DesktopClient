using System.Windows.Controls;
using Microsoft.Windows.Shell;

namespace Paragon.Runtime.WPF
{
    public class WindowButton : Button
    {
        public WindowButton()
        {
            Focusable = false;
        }

        public override void EndInit()
        {
            base.EndInit();

            if (WindowsVersion.IsWin7OrNewer)
            {
                EnableCustomChromeHitTest();
            }
        }

        private void EnableCustomChromeHitTest()
        {
            WindowChrome.SetIsHitTestVisibleInChrome(this, true);
        }
    }
}