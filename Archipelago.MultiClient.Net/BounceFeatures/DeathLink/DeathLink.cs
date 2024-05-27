using Archipelago.MultiClient.Net.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
	/// <summary>
	/// A DeathLink object that gets sent and received via bounce packets.
	/// </summary>
    public class DeathLink : IEquatable<DeathLink>
    {
	    /// <summary>
	    /// The Timestamp of the created DeathLink object
	    /// </summary>
        public DateTime Timestamp { get; internal set; }
	    /// <summary>
	    /// The name of the player who sent the DeathLink
	    /// </summary>
        public string Source { get; }
	    /// <summary>
	    /// The full text to print for players receiving the DeathLink. Can be null
	    /// </summary>
        public string Cause { get; }
	    
	    /// <summary>
	    /// A DeathLink object that gets sent and received via bounce packets.
	    /// </summary>
	    /// <param name="sourcePlayer">Name of the player sending the DeathLink</param>
	    /// <param name="cause">Optional reason for the DeathLink. Since this is optional it should generally include
	    /// a name as if this entire text is what will be displayed</param>
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

		/// <inheritdoc/>
        public bool Equals(DeathLink other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Source == other.Source
                   && Timestamp.Date.Equals(other.Timestamp.Date)
                   && Timestamp.Hour == other.Timestamp.Hour
                   && Timestamp.Minute == other.Timestamp.Minute
                   && Timestamp.Second == other.Timestamp.Second;
        }

		/// <inheritdoc/>
		public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((DeathLink)obj);
        }

		/// <inheritdoc/>
		public override int GetHashCode()
        {
            unchecked
            {
	            // ReSharper disable once NonReadonlyMemberInGetHashCode
	            var hashCode = Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (Source != null ? Source.GetHashCode() : 0);
                return hashCode;
            }
        }

#pragma warning disable CS1591
		public static bool operator ==(DeathLink lhs, DeathLink rhs) => lhs?.Equals(rhs) ?? rhs is null;
		
		public static bool operator !=(DeathLink lhs, DeathLink rhs) => !(lhs == rhs);
#pragma warning restore CS1591
	}
}
