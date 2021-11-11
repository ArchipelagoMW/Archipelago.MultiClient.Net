using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;

namespace Archipelago.MultiClient.Net
{
    public class ArchipelagoSession
    {
        public ArchipelagoSocketHelper Socket { get; }

        public ReceivedItemsHelper Items { get; }

        public LocationCheckHelper Locations { get; }

        public PlayerHelper Players { get; }

        private IDataPackageCache DataPackageCache { get; }

        private bool expectingDataPackage = false;
        private Action<DataPackage> dataPackageCallback;

        public List<string> Tags = new List<string>();

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations,
                                    PlayerHelper players,
                                    IDataPackageCache cache)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            Players = players;
            DataPackageCache = cache;

            socket.PacketReceived += Socket_PacketReceived;
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.DataPackage:
                {
                    if (expectingDataPackage)
                    {
                        var dataPackagePacket = (DataPackagePacket)packet;

                        DataPackageCache.SaveDataPackageToCache(dataPackagePacket.DataPackage);

                        if (dataPackageCallback != null)
                        {
                            dataPackageCallback(dataPackagePacket.DataPackage);
                        }

                        expectingDataPackage = false;
                        dataPackageCallback = null;
                    }
                    break;
                }
            }
        }

        /// <summary>
        ///     Attempt to log in to the Archipelago server by opening a websocket connection and sending a Connect packet.
        ///     Determining success for this attempt is done by attaching a listener to Socket.PacketReceived and listening for a Connected packet.
        /// </summary>
        /// <param name="game">The game this client is playing.</param>
        /// <param name="name">The slot name of this client.</param>
        /// <param name="version">The minimum AP protocol version this client supports.</param>
        /// <param name="tags">The tags this client supports.</param>
        /// <param name="uuid">The uuid of this client.</param>
        /// <param name="password">The password to connect to this AP room.</param>
        /// <returns>
        ///     <see cref="true"/> if the connection seems to have succeeded and the server socket is reached.
        ///     <see cref="false"/> if the connection to the server socket failed in some way.
        /// </returns>
        /// <remarks>
        ///     The connect attempt is synchronous and will lock for up to 5 seconds as it attempts to connect to the server. 
        ///     Most connections are instantaneous however the timeout is 5 seconds before it returns <see cref="false"/>.
        /// </remarks>
        public bool TryConnectAndLogin(string game, string name, Version version, List<string> tags = null, string uuid = null, string password = null)
        {
            if (uuid == null)
            {
                uuid = Guid.NewGuid().ToString();
            }

            Tags = tags ?? new List<string>();
            
            try
            { 
                Socket.Connect();
                Socket.SendPacket(new ConnectPacket()
                {
                    Game = game,
                    Name = name,
                    Password = password,
                    Tags = Tags,
                    Uuid = uuid,
                    Version = version
                });
                return true;
            }
            catch (ArchipelagoSocketClosedException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Send a ConnectUpdate packet and set the tags for the current connection to the provided <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags">
        ///     The tags with which to overwrite the current slot's tags.
        /// </param>
        public void UpdateTags(List<string> tags)
        {
            Tags = tags ?? new List<string>();

            Socket.SendPacket(new ConnectUpdatePacket
            {
                Tags = Tags
            });
        }
    }
}