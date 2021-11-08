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
            var items = new ReceivedItemsHelper(socket, new DataPackageFileSystemCache(socket));
            return new ArchipelagoSession(socket, items);
        }
    }
}
