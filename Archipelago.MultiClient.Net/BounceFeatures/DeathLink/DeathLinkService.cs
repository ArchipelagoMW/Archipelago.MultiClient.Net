using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.BounceFeatures.DeathLink
{
    public class DeathLinkService
    {
        readonly IArchipelagoSocketHelper socket;

        public delegate void DeathLinkReceivedHandler(DeathLink deathLink);
        public event DeathLinkReceivedHandler OnDeathLinkReceived;

        private DataStorageHelper logger;
        private int slot;

        internal DeathLinkService(IArchipelagoSocketHelper socket)
        {
            this.socket = socket;
            socket.PacketReceived += OnPacketReceived;

            logger = new DataStorageHelper(socket);
        }

        private void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket connected:
                    slot = connected.Slot;
                    logger["DeathLinkReceived" + slot].Initialize(new string[0]);
                    logger["DeathLinkSend" + slot].Initialize(new string[0]);
                    logger["DeathLinkFailed"].Initialize(new string[0]);
                    break;

                case BouncedPacket bouncedPacket when bouncedPacket.Tags.Contains("DeathLink"):
                    if (DeathLink.TryParse(bouncedPacket.Data, out var deathLink))
                    {
                        logger["DeathLinkReceived" + slot] += new [] { $"Parsed on {DateTime.UtcNow:u}: {JObject.FromObject(bouncedPacket)}" };

                        if (OnDeathLinkReceived != null)
                        {
                            OnDeathLinkReceived(deathLink);
                        }
                    }
                    else
                    {
                        logger["DeathLinkReceived" + slot] += new[] { $"Failed on {DateTime.UtcNow:u}: {JObject.FromObject(bouncedPacket)}" };
                        logger["DeathLinkFailed"] += new[] { $"Failed for slot {slot} on {DateTime.UtcNow:u}: {JObject.FromObject(bouncedPacket)}" };
                    }
                    break;
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

            logger["DeathLinkSend" + slot] += new[] { $"Send on {DateTime.UtcNow:u}: {JObject.FromObject(bouncePacket)}" };

            socket.SendPacketAsync(bouncePacket);
        }
    }
}
