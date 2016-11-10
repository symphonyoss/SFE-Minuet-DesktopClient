//
// DO NOT MODIFY! THIS IS AUTOGENERATED FILE!
//
namespace Xilium.CefGlue.Interop
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Security;
    
    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    [SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable")]
    internal unsafe struct cef_v8stack_trace_t
    {
        internal cef_base_t _base;
        internal IntPtr _is_valid;
        internal IntPtr _get_frame_count;
        internal IntPtr _get_frame;
        
        // GetCurrent
        [DllImport(libcef.DllName, EntryPoint = "cef_v8stack_trace_get_current", CallingConvention = libcef.CEF_CALL)]
        public static extern cef_v8stack_trace_t* get_current(int frame_limit);
        
        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
        #if !DEBUG
        [SuppressUnmanagedCodeSecurity]
        #endif
        private delegate void add_ref_delegate(cef_v8stack_trace_t* self);
        
        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
        #if !DEBUG
        [SuppressUnmanagedCodeSecurity]
        #endif
        private delegate int release_delegate(cef_v8stack_trace_t* self);
        
        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
        #if !DEBUG
        [SuppressUnmanagedCodeSecurity]
        #endif
        private delegate int has_one_ref_delegate(cef_v8stack_trace_t* self);
        
        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
        #if !DEBUG
        [SuppressUnmanagedCodeSecurity]
        #endif
        private delegate int is_valid_delegate(cef_v8stack_trace_t* self);
        
        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
        #if !DEBUG
        [SuppressUnmanagedCodeSecurity]
        #endif
        private delegate int get_frame_count_delegate(cef_v8stack_trace_t* self);
        
        [UnmanagedFunctionPointer(libcef.CEF_CALLBACK)]
        #if !DEBUG
        [SuppressUnmanagedCodeSecurity]
        #endif
        private delegate cef_v8stack_frame_t* get_frame_delegate(cef_v8stack_trace_t* self, int index);
        
        // AddRef
        private static IntPtr _p0;
        private static add_ref_delegate _d0;
        
        public static void add_ref(cef_v8stack_trace_t* self)
        {
            add_ref_delegate d;
            var p = self->_base._add_ref;
            if (p == _p0) { d = _d0; }
            else
            {
                d = (add_ref_delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(add_ref_delegate));
                if (_p0 == IntPtr.Zero) { _d0 = d; _p0 = p; }
            }
            d(self);
        }
        
        // Release
        private static IntPtr _p1;
        private static release_delegate _d1;
        
        public static int release(cef_v8stack_trace_t* self)
        {
            release_delegate d;
            var p = self->_base._release;
            if (p == _p1) { d = _d1; }
            else
            {
                d = (release_delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(release_delegate));
                if (_p1 == IntPtr.Zero) { _d1 = d; _p1 = p; }
            }
            return d(self);
        }
        
        // HasOneRef
        private static IntPtr _p2;
        private static has_one_ref_delegate _d2;
        
        public static int has_one_ref(cef_v8stack_trace_t* self)
        {
            has_one_ref_delegate d;
            var p = self->_base._has_one_ref;
            if (p == _p2) { d = _d2; }
            else
            {
                d = (has_one_ref_delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(has_one_ref_delegate));
                if (_p2 == IntPtr.Zero) { _d2 = d; _p2 = p; }
            }
            return d(self);
        }
        
        // IsValid
        private static IntPtr _p3;
        private static is_valid_delegate _d3;
        
        public static int is_valid(cef_v8stack_trace_t* self)
        {
            is_valid_delegate d;
            var p = self->_is_valid;
            if (p == _p3) { d = _d3; }
            else
            {
                d = (is_valid_delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(is_valid_delegate));
                if (_p3 == IntPtr.Zero) { _d3 = d; _p3 = p; }
            }
            return d(self);
        }
        
        // GetFrameCount
        private static IntPtr _p4;
        private static get_frame_count_delegate _d4;
        
        public static int get_frame_count(cef_v8stack_trace_t* self)
        {
            get_frame_count_delegate d;
            var p = self->_get_frame_count;
            if (p == _p4) { d = _d4; }
            else
            {
                d = (get_frame_count_delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(get_frame_count_delegate));
                if (_p4 == IntPtr.Zero) { _d4 = d; _p4 = p; }
            }
            return d(self);
        }
        
        // GetFrame
        private static IntPtr _p5;
        private static get_frame_delegate _d5;
        
        public static cef_v8stack_frame_t* get_frame(cef_v8stack_trace_t* self, int index)
        {
            get_frame_delegate d;
            var p = self->_get_frame;
            if (p == _p5) { d = _d5; }
            else
            {
                d = (get_frame_delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(get_frame_delegate));
                if (_p5 == IntPtr.Zero) { _d5 = d; _p5 = p; }
            }
            return d(self, index);
        }
        
    }
}
