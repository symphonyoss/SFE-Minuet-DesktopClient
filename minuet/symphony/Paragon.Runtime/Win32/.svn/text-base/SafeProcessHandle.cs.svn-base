using System;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace Paragon.Runtime.Win32
{
    internal class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private SafeProcessHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        public static SafeProcessHandle OpenProcess(int pid, ProcessAccessFlags access)
        {
            return new SafeProcessHandle(NativeMethods.OpenProcess(access, false, pid));
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }
    }
}