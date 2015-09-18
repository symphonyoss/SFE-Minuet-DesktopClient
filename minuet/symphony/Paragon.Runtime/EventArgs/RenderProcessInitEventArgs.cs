using System;
using Xilium.CefGlue;

namespace Paragon.Runtime
{
    public class RenderProcessInitEventArgs : EventArgs, IDisposable
    {
        private bool _disposed;
        private CefListValue _initArgs;
        private readonly object _lock = new object();

        public RenderProcessInitEventArgs(CefListValue initArgs)
        {
            if(initArgs == null)
                throw new ArgumentNullException("initArgs");

            _initArgs = initArgs;
            _disposed = false;
        }

        public CefListValue InitArgs
        {
            get
            {
                lock (_lock)
                {
                    if (_disposed)
                        throw new ObjectDisposedException("InitArgs was disposed");

                    return _initArgs;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        if (_initArgs != null)
                            _initArgs.Dispose();
                    }

                    _initArgs = null;
                    _disposed = true;
                }    
            }
        }
        
    }
}