using System;

namespace Archipelago.MultiClient.Net.Converters
{
    public static class UnixTimeConverter
    {
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            if (unixTimeStamp > 1000000000000) //its a 64-bit unix timestamp
                unixTimeStamp /= 1000;

            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }

        public static double ToUnixTimeStamp(this DateTime dateTime)
        {
            var utcEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (dateTime - utcEpoch).TotalMilliseconds / 1000;
        }
    }
}
