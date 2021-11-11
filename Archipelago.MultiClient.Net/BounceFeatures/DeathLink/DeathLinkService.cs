using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public class DeathLinkService
    {
        readonly ArchipelagoSession session;

        public delegate void DeathLinkReceivedHandler(DeathLink deathLink);
        public event DeathLinkReceivedHandler OnDeathLinkReceived;

        internal DeathLinkService(ArchipelagoSession session)
        {
            this.session = session;
            session.Socket.PacketReceived += OnPacketReceived;
        }

        void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            if (packet.PacketType != ArchipelagoPacketType.Bounced)
            {
                return;
            }

            var bouncedPacket = packet as BouncedPacket;
            if (bouncedPacket == null
                || !bouncedPacket.Tags.Contains("DeathLink")
                || !DeathLink.TryParse(bouncedPacket.Data, out var deathLink))
            {
                return;
            }

            OnDeathLinkReceived(deathLink);
        }

        /// <summary>
        ///     Formats and sends a Bounce packet using the provided <paramref name="deathLink"/> object.
        /// </summary>
        /// <param name="deathLink">
        ///     <see cref="DeathLink"/> object containing the information of the death which occurred.
        ///     Must at least contain the <see cref="DeathLink.Timestamp"/> and <see cref="DeathLink.Source"/>.
        /// </param>
        public void SendDeathLink(DeathLink deathLink)
        {
            var bouncePacket = new BouncePacket
            {
                Tags = new List<string> { "DeathLink" },
                Data = new Dictionary<string, object>
                {
                    {"time", DeathLink.DateTimeToUnixTimeStamp(deathLink.Timestamp)},
                    {"source", deathLink.Source},
                }
            };

            if (deathLink.Cause != null)
            {
                bouncePacket.Data.Add("cause", deathLink.Cause);
            }

            session.Socket.SendPacketAsync(bouncePacket);
        }
    }
}