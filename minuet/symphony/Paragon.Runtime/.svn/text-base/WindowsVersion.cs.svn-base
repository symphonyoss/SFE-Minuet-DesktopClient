using System;

namespace Paragon.Runtime
{
    internal static class WindowsVersion
    {
        private static bool? _isWin7OrNewer;

        public static bool IsWin7OrNewer
        {
            get
            {
                if (_isWin7OrNewer.HasValue)
                {
                    return _isWin7OrNewer.Value;
                }

                var major = Environment.OSVersion.Version.Major;
                var minor = Environment.OSVersion.Version.Minor;

                // Win7 (and Windows Server 2008 R2, the server equivalent) starts from 6.1.
                if (major < 6)
                {
                    // Major version is less than 5 -> not Win7.
                    _isWin7OrNewer = false;
                }
                else if (major == 6)
                {
                    // Major version is 6. If the minor version is at least 1 then 
                    // we are on Win7 or a later OS.
                    _isWin7OrNewer = minor >= 1;
                }
                else
                {
                    // Major version is greater than 6. We are on a Win7 or later OS.
                    _isWin7OrNewer = true;
                }

                return _isWin7OrNewer.Value;
            }
        }
    }
}