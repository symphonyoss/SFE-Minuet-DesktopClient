using System.Windows.Controls;
using Microsoft.Windows.Shell;

namespace Paragon.Runtime.WPF
{
    internal class WindowButton : Button
    {
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