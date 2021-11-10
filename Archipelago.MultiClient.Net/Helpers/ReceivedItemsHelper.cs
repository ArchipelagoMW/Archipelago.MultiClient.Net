using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class ReceivedItemsHelper
    {
        private readonly ArchipelagoSocketHelper socket;
        private readonly IDataPackageCache dataPackageCache;
        private readonly DataPackage dataPackage;
        private int itemsReceivedIndex = 0;
        private readonly Queue<NetworkItem> itemQueue = new Queue<NetworkItem>();
        private readonly List<NetworkItem> allItemsReceived = new List<NetworkItem>();
        private readonly Dictionary<int, string> itemLookupCache = new Dictionary<int, string>();
        private readonly object itemQueueLockObject = new object();

        public int Index => itemsReceivedIndex;
        public ReadOnlyCollection<NetworkItem> AllItemsReceived => GetReceivedItems();

        ReadOnlyCollection<NetworkItem> GetReceivedItems()
        {
            lock (itemQueueLockObject)
            {
                return new ReadOnlyCollection<NetworkItem>(allItemsReceived);
            }
        }

        public delegate void ItemReceivedHandler();
        public event ItemReceivedHandler ItemReceived;

        internal ReceivedItemsHelper(ArchipelagoSocketHelper socket, IDataPackageCache dataPackageCache)
        {
            this.socket = socket;
            this.dataPackageCache = dataPackageCache;
            socket.PacketReceived += Socket_PacketReceived;

            dataPackageCache.TryGetDataPackageFromCache(out dataPackage);
        }

        /// <summary>
        ///     Peek the next item on the queue to be handled. 
        ///     The item will remain on the queue until dequeued with <see cref="DequeueItem"/>.
        /// </summary>
        /// <returns>
        ///     The next item to be handled as a <see cref="NetworkItem"/>.
        /// </returns>
        public NetworkItem PeekItem()
        {
            lock (itemQueueLockObject)
            {
                return itemQueue.Peek();
            }
        }

        /// <summary>
        ///     Peek the name of next item on the queue to be handled.
        /// </summary>
        /// <returns>
        ///     The name of the item.
        /// </returns>
        public string PeekItemName()
        {
            lock (itemQueueLockObject)
            {
                var item = itemQueue.Peek();
                return GetItemName(item.Item);
            }
        }

        /// <summary>
        ///     Dequeues and returns the next item on the queue to be handled.
        /// </summary>
        /// <returns>
        ///     The next item to be handled as a <see cref="NetworkItem"/>.
        /// </returns>
        public NetworkItem DequeueItem()
        {
            lock (itemQueueLockObject)
            {
                itemsReceivedIndex++;
                return itemQueue.Dequeue();
            }
        }

        /// <summary>
        ///     Perform a lookup using the DataPackage sent as a source of truth to lookup a particular item id for a particular game.
        /// </summary>
        /// <param name="id">
        ///     Id of the item to lookup.
        /// </param>
        /// <returns>
        ///     The name of the item as a string.
        /// </returns>
        public string GetItemName(int id)
        {
            if (itemLookupCache.TryGetValue(id, out var name))
            {
                return name;
            }

            var gameDataContainingId = dataPackage.Games.Single(x => x.Value.ItemLookup.ContainsValue(id));
            var gameDataItemLookup = gameDataContainingId.Value.ItemLookup.ToDictionary(x => x.Value, x => x.Key);
            foreach (var kvp in gameDataItemLookup)
            {
                itemLookupCache.Add(kvp.Key, kvp.Value);
            }

            try
            {
                return itemLookupCache[id];
            }
            catch (KeyNotFoundException e)
            {
                throw new UnknownItemIdException($"Attempt to look up item id `{id}` failed.", e);
            }
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.ReceivedItems:
                {
                    var receivedItemsPacket = (ReceivedItemsPacket)packet;

                    if (itemsReceivedIndex != receivedItemsPacket.Index)
                    {
                        socket.SendPacket(new SyncPacket());
                        break;
                    }

                    if (receivedItemsPacket.Index == 0)
                    {
                        PerformResynchronization(receivedItemsPacket);
                        break;
                    }

                    lock (itemQueueLockObject)
                    {
                        foreach (var item in receivedItemsPacket.Items)
                        {
                            allItemsReceived.Add(item);
                            itemQueue.Enqueue(item);
                            itemsReceivedIndex++;

                            if (ItemReceived != null)
                            {
                                ItemReceived();
                            }
                        }
                    }
                    break;
                }
            }
        }

        private void PerformResynchronization(ReceivedItemsPacket receivedItemsPacket)
        {
            lock (itemQueueLockObject)
            {
                itemQueue.Clear();
                allItemsReceived.Clear();
                itemsReceivedIndex = 0;
                foreach (var item in receivedItemsPacket.Items)
                {
                    itemQueue.Enqueue(item);
                    allItemsReceived.Add(item);
                    itemsReceivedIndex++;
                }
            }
        }
    }
}
