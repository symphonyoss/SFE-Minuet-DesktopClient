namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// WaitableEvent is a thread synchronization tool that allows one thread to wait
    /// for another thread to finish some work. This is equivalent to using a
    /// Lock+ConditionVariable to protect a simple boolean value. However, using
    /// WaitableEvent in conjunction with a Lock to wait for a more complex state
    /// change (e.g., for an item to be added to a queue) is not recommended. In that
    /// case consider using a ConditionVariable instead of a WaitableEvent. It is
    /// safe to create and/or signal a WaitableEvent from any thread. Blocking on a
    /// WaitableEvent by calling the *Wait() methods is not allowed on the browser
    /// process UI or IO threads.
    /// </summary>
    public sealed unsafe partial class CefWaitableEvent
    {
        /// <summary>
        /// Create a new waitable event. If |automatic_reset| is true then the event
        /// state is automatically reset to un-signaled after a single waiting thread
        /// has been released; otherwise, the state remains signaled until Reset() is
        /// called manually. If |initially_signaled| is true then the event will start
        /// in the signaled state.
        /// </summary>
        public static cef_waitable_event_t* CreateWaitableEvent(int automatic_reset, int initially_signaled)
        {
            throw new NotImplementedException(); // TODO: CefWaitableEvent.CreateWaitableEvent
        }
        
        /// <summary>
        /// Put the event in the un-signaled state.
        /// </summary>
        public void Reset()
        {
            throw new NotImplementedException(); // TODO: CefWaitableEvent.Reset
        }
        
        /// <summary>
        /// Put the event in the signaled state. This causes any thread blocked on Wait
        /// to be woken up.
        /// </summary>
        public void Signal()
        {
            throw new NotImplementedException(); // TODO: CefWaitableEvent.Signal
        }
        
        /// <summary>
        /// Returns true if the event is in the signaled state, else false. If the
        /// event was created with |automatic_reset| set to true then calling this
        /// method will also cause a reset.
        /// </summary>
        public int IsSignaled()
        {
            throw new NotImplementedException(); // TODO: CefWaitableEvent.IsSignaled
        }
        
        /// <summary>
        /// Wait indefinitely for the event to be signaled. This method will not return
        /// until after the call to Signal() has completed. This method cannot be
        /// called on the browser process UI or IO threads.
        /// </summary>
        public void Wait()
        {
            throw new NotImplementedException(); // TODO: CefWaitableEvent.Wait
        }
        
        /// <summary>
        /// Wait up to |max_ms| milliseconds for the event to be signaled. Returns true
        /// if the event was signaled. A return value of false does not necessarily
        /// mean that |max_ms| was exceeded. This method will not return until after
        /// the call to Signal() has completed. This method cannot be called on the
        /// browser process UI or IO threads.
        /// </summary>
        public int TimedWait(long max_ms)
        {
            throw new NotImplementedException(); // TODO: CefWaitableEvent.TimedWait
        }
        
    }
}
