namespace Microsoft.OpenPublishing.Build.Common
{
    using System;

    public static class DateTimeExtensions
    {
        public static readonly DateTime UnixEpochDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTimeSeconds(this long seconds)
        {
            return UnixEpochDate.AddSeconds(seconds);
        }

        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpochDate).TotalSeconds;
        }
    }
}