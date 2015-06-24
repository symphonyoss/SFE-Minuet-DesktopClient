using System;
using System.Collections.Generic;
using System.Text;

namespace Xilium.CefGlue
{
    public enum CefWindowOpenDisposition
    {
        Unknown,
        SupressOpen,
        CurrentTab,
        SingletonTab,
        NewForegroundTab,
        NewBackgroundTab,
        NewPopup,
        NewWindow,
        SaveToDisk,
        OffTheRecord,
        IgnoreAction
    }
}
