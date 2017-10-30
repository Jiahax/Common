namespace Common
{
    using System;
    using System.Runtime.InteropServices;

    public static class HighPrecisionDateTimeHelper
    {
        private static readonly bool IsSystemTimeAvailable;

        [DllImport("Kernel32.dll")]
        private static extern bool GetSystemTimePreciseAsFileTime(out long systemTime);

        static HighPrecisionDateTimeHelper()
        {
            try
            {
                long systemTime;
                GetSystemTimePreciseAsFileTime(out systemTime);
                IsSystemTimeAvailable = true;
            }
            catch (Exception)
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/hh706895(v=vs.85).aspx
                // Minimum supported version: Windows 8 or Windows Server 2012
                IsSystemTimeAvailable = false;
            }
        }

        /// <summary>
        /// If system time available, return current system date and time with the highest possible level of precision, less then 1us.
        /// If system time not available, return DateTime.UtcNow
        /// </summary>
        /// <returns>UTC date and time</returns>
        public static DateTime GetUtcNow()
        {
            if (IsSystemTimeAvailable)
            {
                long systemTime;
                GetSystemTimePreciseAsFileTime(out systemTime);
                return DateTime.FromFileTimeUtc(systemTime);
            }

            return DateTime.UtcNow;
        }
    }
}
