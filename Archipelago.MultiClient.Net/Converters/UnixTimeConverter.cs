using System;

namespace Archipelago.MultiClient.Net.Converters
{
    public static class UnixTimeConverter
    {
        static DateTime UtcEpoch =>
#if !NET6_0
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
#else
            DateTime.UnixEpoch;
#endif

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            if (unixTimeStamp > 1000000000000) //its a 64-bit unix timestamp
                unixTimeStamp /= 1000;

            return UtcEpoch.AddSeconds(unixTimeStamp);
        }

        public static double ToUnixTimeStamp(this DateTime dateTime) => (dateTime - UtcEpoch).TotalMilliseconds / 1000;
    }
}
