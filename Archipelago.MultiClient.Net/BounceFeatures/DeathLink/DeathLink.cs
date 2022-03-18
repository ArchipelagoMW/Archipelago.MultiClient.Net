using Archipelago.MultiClient.Net.Converters;
using Newtonsoft.Json.Linq;
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

        internal static bool TryParse(Dictionary<string, JToken> data, out DeathLink deathLink)
        {
            try
            {
                if (!data.TryGetValue("time", out JToken timeStampToken) || !data.TryGetValue("source", out JToken sourceToken))
                {
                    deathLink = null;
                    return false;
                }

                string cause = null;
                if (data.TryGetValue("cause", out JToken causeToken))
                {
                    cause = causeToken.ToString();
                }

                deathLink = new DeathLink(sourceToken.ToString(), cause) {
                    Timestamp = UnixTimeConverter.UnixTimeStampToDateTime(timeStampToken.ToObject<double>()),
                };
                return true;
            }
            catch
            {
                deathLink = null;
                return false;
            }
        }
    }
}
