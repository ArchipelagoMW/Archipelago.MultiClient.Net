using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Enums;
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

        private IDataPackageCache DataPackageCache { get; }
        
        private bool expectingDataPackage = false;
        private Action<DataPackage> dataPackageCallback;

        public List<string> Tags = new List<string>();

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations,
                                    IDataPackageCache cache)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
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
        ///     Attempts to retrieve the datapackage from cache or from the server, if there is no cached version.
        ///     Calls a callback method when retrieval is successful.
        ///     If the socket is not open AND there is no file cache when this method is called, then a GetDataPackage packet
        ///     will be queued up to be sent to the server as soon as a connection is made.
        /// </summary>
        /// <param name="callback">
        ///     Action to call when the datapackage is received or retrieved from cache.
        /// </param>
        public void GetDataPackageAsync(Action<DataPackage> callback)
        {
            if (DataPackageCache.TryGetDataPackageFromCache(out var package))
            {
                if (callback != null)
                {
                    callback(package);
                }
            }
            else
            {
                if (Socket.Connected)
                {
                    Socket.SendPacket(new GetDataPackagePacket());
                    expectingDataPackage = true;
                    dataPackageCallback = callback;
                }
                else
                {
                    Action socketOpenCallback = null;
                    socketOpenCallback = () =>
                    {
                        Socket.SendPacket(new GetDataPackagePacket());
                        Socket.SocketOpened -= socketOpenCallback;
                    };
                    Socket.SocketOpened += socketOpenCallback;
                    expectingDataPackage = true;
                    dataPackageCallback = callback;
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
        public void AttemptConnectAndLogin(string game, string name, Version version, List<string> tags = null, string uuid = null, string password = null)
        {
            if (uuid == null)
            {
                uuid = Guid.NewGuid().ToString();
            }

	        Tags = tags ?? new List<string>();

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
        }

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