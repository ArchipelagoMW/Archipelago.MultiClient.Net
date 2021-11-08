using Archipelago.MultiClient.Net.Cache;
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

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            DataPackageCache = new DataPackageFileSystemCache(socket);
        }

        /// <summary>
        /// Attempts to retrieve the datapackage from the cache. If there is no package in the cache
        /// then this method will initiate a GetDataPackage from the server and return null.
        /// You can call this method back after some period of time to retrieve the datapackage, in order
        /// to give the server time to respond to the get request.
        /// 
        /// Generally, it should not be the case that this method returns null due to all of the automated datapackage
        /// retrieval that goes on within the library.
        /// </summary>
        public DataPackage GetDataPackage()
        {
            if (DataPackageCache.TryGetDataPackageFromCache(out var package))
            {
                return package;
            }
            else
            {
                Socket.SendPacket(new GetDataPackagePacket());
                return null;
            }
        }
    }
}