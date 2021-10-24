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

        private DataPackageFileSystemCache DataPackageCache { get; }

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items)
        {
            Socket = socket;
            Items = items;
            DataPackageCache = new DataPackageFileSystemCache(socket);
        }

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