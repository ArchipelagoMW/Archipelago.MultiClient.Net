using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public class DeathLinkService
    {
        readonly IArchipelagoSocketHelper socket;

        public delegate void DeathLinkReceivedHandler(DeathLink deathLink);
        public event DeathLinkReceivedHandler OnDeathLinkReceived;

        internal DeathLinkService(IArchipelagoSocketHelper socket)
        {
            this.socket = socket;
            socket.PacketReceived += OnPacketReceived;
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

            if (OnDeathLinkReceived != null)
            { 
                OnDeathLinkReceived(deathLink);
            }
        }

        /// <summary>
        ///     Formats and sends a Bounce packet using the provided <paramref name="deathLink"/> object.
        /// </summary>
        /// <param name="deathLink">
        ///     <see cref="DeathLink"/> object containing the information of the death which occurred.
        ///     Must at least contain the <see cref="DeathLink.Source"/>.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive
        /// </exception>
        public void SendDeathLink(DeathLink deathLink)
        {
            var bouncePacket = new BouncePacket
            {
                Tags = new List<string> { "DeathLink" },
                Data = new Dictionary<string, JToken>
                {
                    {"time", deathLink.Timestamp.ToUnixTimeStamp()},
                    {"source", deathLink.Source},
                }
            };

            if (deathLink.Cause != null)
            {
                bouncePacket.Data.Add("cause", deathLink.Cause);
            }

            socket.SendPacketAsync(bouncePacket);
        }
    }
}
