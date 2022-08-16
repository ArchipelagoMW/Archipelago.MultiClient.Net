using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.ConcurrentCollection;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#if !NET35
using System.Collections.Concurrent;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
    public interface IReceivedItemsHelper
    {
        string GetItemName(long id);
    }

    public class ReceivedItemsHelper : IReceivedItemsHelper
    {
        private readonly IArchipelagoSocketHelper socket;
        private readonly ILocationCheckHelper locationsHelper;
        private readonly IDataPackageCache dataPackageCache;

        private ConcurrentQueue<NetworkItem> itemQueue = new ConcurrentQueue<NetworkItem>();

        private readonly IConcurrentList<NetworkItem> allItemsReceived = new ConcurrentList<NetworkItem>();

        private Dictionary<long, string> itemLookupCache = new Dictionary<long, string>();
        
        public int Index => allItemsReceived.Count;
        public ReadOnlyCollection<NetworkItem> AllItemsReceived => allItemsReceived.AsReadOnlyCollection();

        public delegate void ItemReceivedHandler(ReceivedItemsHelper helper);
        public event ItemReceivedHandler ItemReceived;

        internal ReceivedItemsHelper(IArchipelagoSocketHelper socket, ILocationCheckHelper locationsHelper, IDataPackageCache dataPackageCache)
        {
            this.socket = socket;
            this.locationsHelper = locationsHelper;
            this.dataPackageCache = dataPackageCache;

            socket.PacketReceived += Socket_PacketReceived;
        }

        /// <summary>
        ///     Check whether there are any items in the queue. 
        /// </summary>
        /// <returns>
        ///     True if the queue is not empty, otherwise false.
        /// </returns>
        public bool Any()
        {
            return !itemQueue.IsEmpty;
        }

        /// <summary>
        ///     Peek the next item on the queue to be handled. 
        ///     The item will remain on the queue until dequeued with <see cref="DequeueItem"/>.
        /// </summary>
        /// <returns>
        ///     The next item to be handled as a <see cref="NetworkItem"/>, or null if no such item is found.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        ///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
        /// </exception>
        public NetworkItem PeekItem()
        {
            itemQueue.TryPeek(out var item);
            return item;
        }

        /// <summary>
        ///     Peek the name of next item on the queue to be handled.
        /// </summary>
        /// <returns>
        ///     The name of the item as a string, or null if no such item is found.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        ///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
        /// </exception>
        public string PeekItemName()
        {
            itemQueue.TryPeek(out var item);
            return GetItemName(item.Item);
        }

        /// <summary>
        ///     Dequeues and returns the next item on the queue to be handled.
        /// </summary>
        /// <returns>
        ///     The next item to be handled as a <see cref="NetworkItem"/>.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        ///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
        /// </exception>
        public NetworkItem DequeueItem()
        {
            itemQueue.TryDequeue(out var item);
            return item;
        }

        /// <summary>
        ///     Perform a lookup using the DataPackage sent as a source of truth to lookup a particular item id for a particular game.
        /// </summary>
        /// <param name="id">
        ///     Id of the item to lookup.
        /// </param>
        /// <returns>
        ///     The name of the item as a string, or null if no such item is found.
        /// </returns>
        public string GetItemName(long id)
        {
            if (itemLookupCache.TryGetValue(id, out var name))
            {
                return name;
            }

            if (!dataPackageCache.TryGetDataPackageFromCache(out var dataPackage))
            {
                return null;
            }
            
            itemLookupCache = dataPackage.Games.Select(x => x.Value).SelectMany(x => x.ItemLookup).ToDictionary(x => x.Value, x => x.Key);

            return itemLookupCache.TryGetValue(id, out var itemName)
                ? itemName
                : null;
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.ReceivedItems:
                {
                    var receivedItemsPacket = (ReceivedItemsPacket)packet;

                    if (receivedItemsPacket.Index == 0)
                    {
                        PerformResynchronization(receivedItemsPacket);
                        break;
                    }

                    if (allItemsReceived.Count != receivedItemsPacket.Index)
                    {
                        socket.SendPacket(new SyncPacket());
                        locationsHelper.CompleteLocationChecks();
                        break;
                    }

                    foreach (var item in receivedItemsPacket.Items)
                    {
                        allItemsReceived.Add(item);
                        itemQueue.Enqueue(item);

                        if (ItemReceived != null)
                        {
                            ItemReceived(this);
                        }
                    }
                    break;
                }
            }
        }

        private void PerformResynchronization(ReceivedItemsPacket receivedItemsPacket)
        {
            var previouslyReceived = allItemsReceived.AsReadOnlyCollection();

#if NET35
            itemQueue.Clear();
#else
            itemQueue = new ConcurrentQueue<NetworkItem>();
#endif
            allItemsReceived.Clear();

            foreach (var item in receivedItemsPacket.Items)
            {
                itemQueue.Enqueue(item);
                allItemsReceived.Add(item);

                if (ItemReceived != null && !previouslyReceived.Contains(item))
                {
                    ItemReceived(this);
                }
            }
        }
    }
}
