using System;
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

        private DataPackageFileSystemCache DataPackageCache { get; }
        
        private bool expectingDataPackage = false;
        private Action<DataPackage> dataPackageCallback;

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            DataPackageCache = new DataPackageFileSystemCache(socket);

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
        ///     Attempts to retrieve the datapackage from the cache. If there is no package in the cache
        ///     then this method will initiate a GetDataPackage and then call your callback when the packet is received.
        /// 
        ///     Generally, it should not be the case that this method returns null due to all of the automated datapackage
        ///     retrieval that goes on within the library.
        /// </summary>
        /// <param name="callback">
        ///     Action to call when the datapackage is received, should it be null or otherwise unretrieved.
        /// </param>
        public DataPackage GetDataPackage(Action<DataPackage> callback = null)
        {
            if (DataPackageCache.TryGetDataPackageFromCache(out var package))
            {
                return package;
            }
            else
            {
                Socket.SendPacket(new GetDataPackagePacket());
                expectingDataPackage = true;
                dataPackageCallback = callback;
                return null;
            }
        }
    }
}