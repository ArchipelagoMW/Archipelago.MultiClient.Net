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

        internal static bool TryParse(Dictionary<string, object> data, out DeathLink deathLink)
        {
            try
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

                deathLink = new DeathLink(source.ToString(), cause) {
                    Timestamp = UnixTimeStampToDateTime((double)timeStamp),
                };
                return true;
            }
            catch
            {
                deathLink = null;
                return false;
            }
        }

        internal static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp);
            return dateTime;
        }

        internal static double DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            var utcEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (dateTime - utcEpoch).TotalMilliseconds / 1000;
        }
    }
}
