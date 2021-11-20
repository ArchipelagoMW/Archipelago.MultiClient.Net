using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public class DeathLink
    {
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public string Cause { get; set; }

        public DeathLink(string sourcePlayer, string cause = null)
        {
            Timestamp = DateTime.UtcNow;
            Source = sourcePlayer;
            Cause = cause;
        }

        /// <summary>
        ///     Attempt to parse a <see cref="DeathLink"/> object from a data dictionary.
        ///     The dictionary is typically obtained from a Bounced packet.
        /// </summary>
        public static bool TryParse(Dictionary<string, object> data, out DeathLink deathLink)
        {
            if (!data.TryGetValue("time", out object timeStamp) || !data.TryGetValue("source", out object source))
            {
                deathLink = null;
                return false;
            }

            string cause = null;
            if (data.TryGetValue("cause", out object causeObject))
            {
                cause = causeObject.ToString();
            }

            deathLink = new DeathLink((string)source, cause)
            {
                Timestamp = UnixTimeStampToDateTime((double)timeStamp),
            };
            return true;
        }

        /// <summary>
        ///     Convert seconds since Unix epoch to a <see cref="DateTime"/>.
        /// </summary>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }

        /// <summary>
        ///     Convert a <see cref="DateTime"/> to seconds since Unix epoch.
        /// </summary>
        public static double DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            var utcEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (dateTime - utcEpoch).TotalMilliseconds / 1000;
        }
    }
}
