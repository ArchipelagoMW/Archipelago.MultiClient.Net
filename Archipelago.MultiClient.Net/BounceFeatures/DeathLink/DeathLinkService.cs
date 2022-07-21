using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json.Linq;
#else
using Newtonsoft.Json.Linq;
#endif
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public class DeathLinkService
    {
        private readonly IArchipelagoSocketHelper socket;
        private readonly IConnectionInfoProvider connectionInfoProvider;
        private readonly DataStorageHelper logger;

        private DeathLink lastSendDeathLink;

        public delegate void DeathLinkReceivedHandler(DeathLink deathLink);
        public event DeathLinkReceivedHandler OnDeathLinkReceived;

        internal DeathLinkService(IArchipelagoSocketHelper socket, IConnectionInfoProvider connectionInfoProvider, DataStorageHelper logger)
        {
            this.socket = socket;
            this.connectionInfoProvider = connectionInfoProvider;
            this.logger = logger;

            socket.PacketReceived += OnPacketReceived;

            logger[Scope.Slot, "DeathLinkReceived"].Initialize(new string[0]);
            logger[Scope.Slot, "DeathLinkSend"].Initialize(new string[0]);
            logger["FailedDeathLinks"].Initialize(new string[0]);
        }

        private void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case BouncedPacket bouncedPacket when bouncedPacket.Tags.Contains("DeathLink"):
                    if (DeathLink.TryParse(bouncedPacket.Data, out var deathLink))
                    {
                        logger[Scope.Slot, "DeathLinkReceived"] += new [] { $"Parsed on {DateTime.UtcNow:u}: {JObject.FromObject(bouncedPacket)}" };

                        if (lastSendDeathLink != null && lastSendDeathLink == deathLink)
                        {
                            return;
                        }

                        if (OnDeathLinkReceived != null)
                        {
                            OnDeathLinkReceived(deathLink);
                        }
                    }
                    else
                    {
                        logger[Scope.Slot, "DeathLinkReceived"] += new[] { $"Failed on {DateTime.UtcNow:u}: {JObject.FromObject(bouncedPacket)}" };
                        logger["FailedDeathLinks"] += new[] { $"Failed for slot {connectionInfoProvider.Slot} on {DateTime.UtcNow:u}: {JObject.FromObject(bouncedPacket)}" };
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
            {
                bouncePacket.Data.Add("cause", deathLink.Cause);
            }

            logger[Scope.Slot, "DeathLinkSend"] += new[] { $"Send on {DateTime.UtcNow:u}: {JObject.FromObject(bouncePacket)}" };

            lastSendDeathLink = deathLink;

            socket.SendPacketAsync(bouncePacket);
        }

        public void EnabledDeathLink()
        {
            if (Array.IndexOf(connectionInfoProvider.Tags, "DeathLink") == -1)
            {
                connectionInfoProvider.UpdateConnectionOptions(
                    connectionInfoProvider.Tags.Concat(new[] { "DeathLink" }).ToArray());
            }
        }

        public void DisableDeathLink()
        {
            if (Array.IndexOf(connectionInfoProvider.Tags, "DeathLink") == -1)
            {
                return;
            }

            connectionInfoProvider.UpdateConnectionOptions(
                connectionInfoProvider.Tags.Where(t => t != "DeathLink").ToArray());
        }
    }
}
