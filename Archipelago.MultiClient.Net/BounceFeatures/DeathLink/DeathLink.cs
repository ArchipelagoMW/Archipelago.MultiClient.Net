using Archipelago.MultiClient.Net.Converters;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json.Linq;
#else
using Newtonsoft.Json.Linq;
#endif
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public class DeathLink : IEquatable<DeathLink>
    {
        public DateTime Timestamp { get; internal set; }
        public string Source { get; }
        public string Cause { get; }

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

        public bool Equals(DeathLink other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Source == other.Source
                   && Timestamp.Date.Equals(other.Timestamp.Date)
                   && Timestamp.Hour == other.Timestamp.Hour
                   && Timestamp.Minute == other.Timestamp.Minute
                   && Timestamp.Second == other.Timestamp.Second;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((DeathLink)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(DeathLink lhs, DeathLink rhs)
        {
            return lhs?.Equals(rhs) ?? rhs is null;
        }

        public static bool operator !=(DeathLink lhs, DeathLink rhs)
        {
            return !(lhs == rhs);
        }
    }
}
