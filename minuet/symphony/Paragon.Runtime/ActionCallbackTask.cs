using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class ActionCallbackTask : CefTask
    {
        private readonly Action _action;

        public ActionCallbackTask(Action action)
        {
            _action = action;
        }

        protected override void Execute()
        {
            _action();
        }
    }
}