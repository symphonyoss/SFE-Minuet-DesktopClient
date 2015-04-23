namespace Paragon.Runtime.Plugins
{
    /// <summary>
    /// Represents a call from JavaScript to a browser side plugin
    /// </summary>
    public class RenderCallInfo : CallInfo
    {
        public RenderCallInfo(PluginMessage requestMessage, IV8Callback callback)
            : base(requestMessage)
        {
            Callback = callback;
        }

        public IV8Callback Callback { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (Callback != null)
                {
                    Callback.Dispose();
                }
            }
        }
    }
}