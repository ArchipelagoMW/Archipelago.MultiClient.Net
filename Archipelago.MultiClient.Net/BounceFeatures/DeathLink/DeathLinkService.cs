using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public class DeathLinkService
    {
        readonly IArchipelagoSocketHelper socket;
        readonly IConnectionInfoProvider connectionInfoProvider;

        DeathLink lastSendDeathLink;

        /// <summary>
        /// Creates <see cref="OnDeathLinkReceived"/> event for clients to hook into and decide what to do with the
        /// received <see cref="DeathLink"/>
        /// </summary>
        public delegate void DeathLinkReceivedHandler(DeathLink deathLink);
        /// <summary>
        /// Delegate event that supplies the created <see cref="DeathLink"/> whenever one is received from the server
        /// as a bounce packet.
        /// </summary>
        public event DeathLinkReceivedHandler OnDeathLinkReceived;

        internal DeathLinkService(IArchipelagoSocketHelper socket, IConnectionInfoProvider connectionInfoProvider)
        {
            this.socket = socket;
            this.connectionInfoProvider = connectionInfoProvider;

            socket.PacketReceived += OnPacketReceived;
        }

        void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case BouncedPacket bouncedPacket when bouncedPacket.Tags.Contains("DeathLink"):
                    if (DeathLink.TryParse(bouncedPacket.Data, out var deathLink))
                    {
                        if (lastSendDeathLink != null && lastSendDeathLink == deathLink)
                            return;

                        if (OnDeathLinkReceived != null)
                            OnDeathLinkReceived(deathLink);
                    }
                    break;
            }
        }

        // ReSharper disable once UnusedMember.Global
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
                Data = new Dictionary<string, JToken> {
                    {"time", deathLink.Timestamp.ToUnixTimeStamp()},
                    {"source", deathLink.Source},
                }
            };

            if (deathLink.Cause != null)
                bouncePacket.Data.Add("cause", deathLink.Cause);

            lastSendDeathLink = deathLink;

            socket.SendPacketAsync(bouncePacket);
        }

        /// <summary>
        /// Adds "DeathLink" to your <see cref="ArchipelagoSession"/>'s tags and opts you in to receiving
        /// <see cref="OnDeathLinkReceived"/> events
        /// </summary>
        public void EnableDeathLink()
        {
            if (Array.IndexOf(connectionInfoProvider.Tags, "DeathLink") == -1)
                connectionInfoProvider.UpdateConnectionOptions(
                    connectionInfoProvider.Tags.Concat(new[] { "DeathLink" }).ToArray());
        }

        /// <summary>
        /// Removes the "DeathLink" tag from your <see cref="ArchipelagoSession"/> and opts out of further
        /// <see cref="OnDeathLinkReceived"/> events
        /// </summary>
        public void DisableDeathLink()
        {
            if (Array.IndexOf(connectionInfoProvider.Tags, "DeathLink") == -1)
                return;

            connectionInfoProvider.UpdateConnectionOptions(
                connectionInfoProvider.Tags.Where(t => t != "DeathLink").ToArray());
        }
    }
}
