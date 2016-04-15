using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xilium.CefGlue;
using System.Threading;
using Paragon.Plugins;
using Paragon.Runtime;

namespace Paragon.Runtime.WPF
{
    // For CefWebBrowser methods: GetSource and GetText
    // Returns Info as either the source text or text of page
    class SourceStringVisitor : CefStringVisitor
    {
        private static readonly ILogger Logger = ParagonLogManager.GetLogger();
        public ManualResetEvent Done { get; set; }
        public string Info { get; set; }

        public SourceStringVisitor(ManualResetEvent done)
        {
            Done = done;
        }

        //Once the source is received, set Source to be 
        // the string, and notify completion.
        protected override void Visit(string value)
        {
            Logger.Info(value);
            Info = value;
            Done.Set();
        }
    }
}
