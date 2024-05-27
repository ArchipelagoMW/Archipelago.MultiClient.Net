using System;

namespace Archipelago.MultiClient.Net.Converters
{
	/// <summary>
	/// Provides methods to convert between Unix timestamps and DateTime objects.
	/// </summary>
    public static class UnixTimeConverter
    {
        static DateTime UtcEpoch =>
#if !NET6_0
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
#else
            DateTime.UnixEpoch;
#endif
		/// <summary>
		/// Converts a Unix timestamp to a DateTime object.
		/// </summary>
		/// <param name="unixTimeStamp">a unix timestamp either in 64bit format or 32bit format</param>
		/// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            if (unixTimeStamp > 1000000000000) //its a 64-bit unix timestamp
                unixTimeStamp /= 1000;

            return UtcEpoch.AddSeconds(unixTimeStamp);
        }

		/// <summary>
		/// Converts a DateTime object to a Unix timestamp.
		/// </summary>
		/// <param name="dateTime">a datetime timestamp</param>
		/// <returns></returns>
        public static double ToUnixTimeStamp(this DateTime dateTime) => (dateTime - UtcEpoch).TotalMilliseconds / 1000;
    }
}
