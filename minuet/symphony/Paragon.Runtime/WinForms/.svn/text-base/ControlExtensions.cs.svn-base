using System;
using System.Windows.Forms;

namespace Paragon.Runtime.WinForms
{
    internal static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control control, Action a, bool isAsync = false)
        {
            if (control.InvokeRequired)
            {
                if (isAsync)
                {
                    control.BeginInvoke(a);
                }
                else
                {
                    control.Invoke(a);
                }
            }
            else
            {
                a();
            }
        }
    }
}