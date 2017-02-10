//
// DO NOT MODIFY! THIS IS AUTOGENERATED FILE!
//
namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    // Role: PROXY
    public sealed unsafe partial class CefWaitableEvent : IDisposable
    {
        internal static CefWaitableEvent FromNative(cef_waitable_event_t* ptr)
        {
            return new CefWaitableEvent(ptr);
        }
        
        internal static CefWaitableEvent FromNativeOrNull(cef_waitable_event_t* ptr)
        {
            if (ptr == null) return null;
            return new CefWaitableEvent(ptr);
        }
        
        private cef_waitable_event_t* _self;
        
        private CefWaitableEvent(cef_waitable_event_t* ptr)
        {
            if (ptr == null) throw new ArgumentNullException("ptr");
            _self = ptr;
        }
        
        ~CefWaitableEvent()
        {
            if (_self != null)
            {
                Release();
                _self = null;
            }
        }
        
        public void Dispose()
        {
            if (_self != null)
            {
                Release();
                _self = null;
            }
            GC.SuppressFinalize(this);
        }
        
        internal void AddRef()
        {
            cef_waitable_event_t.add_ref(_self);
        }
        
        internal bool Release()
        {
            return cef_waitable_event_t.release(_self) != 0;
        }
        
        internal bool HasOneRef
        {
            get { return cef_waitable_event_t.has_one_ref(_self) != 0; }
        }
        
        internal cef_waitable_event_t* ToNative()
        {
            AddRef();
            return _self;
        }
    }
}
