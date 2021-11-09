using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.MultiClient.Net
{
    public static class ArchipelagoSessionFactory
    {
        public static ArchipelagoSession CreateSession(string hostUrl)
        {
            var socket = new ArchipelagoSocketHelper(hostUrl);
            var dataPackageCache = new DataPackageFileSystemCache(socket);
            
            // Try to get datapackage just to send the GetDataPackage packet in case the cache is empty.
            dataPackageCache.TryGetDataPackageFromCache(out var _);

            var items = new ReceivedItemsHelper(socket, dataPackageCache);
            var locations = new LocationCheckHelper(socket);
            return new ArchipelagoSession(socket, items, locations);
        }
    }
}
