namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;
    
    /// <summary>
    /// A simple thread abstraction that establishes a message loop on a new thread.
    /// The consumer uses CefTaskRunner to execute code on the thread's message loop.
    /// The thread is terminated when the CefThread object is destroyed or Stop() is
    /// called. All pending tasks queued on the thread's message loop will run to
    /// completion before the thread is terminated. CreateThread() can be called on
    /// any valid CEF thread in either the browser or render process. This class
    /// should only be used for tasks that require a dedicated thread. In most cases
    /// you can post tasks to an existing CEF thread instead of creating a new one;
    /// see cef_task.h for details.
    /// </summary>
    public sealed unsafe partial class CefThread
    {
        /// <summary>
        /// Create and start a new thread. This method does not block waiting for the
        /// thread to run initialization. |display_name| is the name that will be used
        /// to identify the thread. |priority| is the thread execution priority.
        /// |message_loop_type| indicates the set of asynchronous events that the
        /// thread can process. If |stoppable| is true the thread will stopped and
        /// joined on destruction or when Stop() is called; otherwise, the the thread
        /// cannot be stopped and will be leaked on shutdown. On Windows the
        /// |com_init_mode| value specifies how COM will be initialized for the thread.
        /// If |com_init_mode| is set to COM_INIT_MODE_STA then |message_loop_type|
        /// must be set to ML_TYPE_UI.
        /// </summary>
        public static cef_thread_t* CreateThread(cef_string_t* display_name, cef_thread_priority_t priority, cef_message_loop_type_t message_loop_type, int stoppable, cef_com_init_mode_t com_init_mode)
        {
            throw new NotImplementedException(); // TODO: CefThread.CreateThread
        }
        
        /// <summary>
        /// Returns the CefTaskRunner that will execute code on this thread's message
        /// loop. This method is safe to call from any thread.
        /// </summary>
        public cef_task_runner_t* GetTaskRunner()
        {
            throw new NotImplementedException(); // TODO: CefThread.GetTaskRunner
        }
        
        /// <summary>
        /// Returns the platform thread ID. It will return the same value after Stop()
        /// is called. This method is safe to call from any thread.
        /// </summary>
        public cef_platform_thread_id_t GetPlatformThreadId()
        {
            throw new NotImplementedException(); // TODO: CefThread.GetPlatformThreadId
        }
        
        /// <summary>
        /// Stop and join the thread. This method must be called from the same thread
        /// that called CreateThread(). Do not call this method if CreateThread() was
        /// called with a |stoppable| value of false.
        /// </summary>
        public void Stop()
        {
            throw new NotImplementedException(); // TODO: CefThread.Stop
        }
        
        /// <summary>
        /// Returns true if the thread is currently running. This method must be called
        /// from the same thread that called CreateThread().
        /// </summary>
        public int IsRunning()
        {
            throw new NotImplementedException(); // TODO: CefThread.IsRunning
        }
        
    }
}
