using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class ReceivedItemsHelper
    {
        private readonly IArchipelagoSocketHelper socket;
        private readonly ILocationCheckHelper locationsHelper;
        private readonly IDataPackageCache dataPackageCache;
        private Queue<NetworkItem> itemQueue = new Queue<NetworkItem>();
        private List<NetworkItem> allItemsReceived = new List<NetworkItem>();
        private Dictionary<long, string> itemLookupCache = new Dictionary<long, string>();
        private object itemQueueLockObject = new object();

        public int Index => allItemsReceived.Count;
        public ReadOnlyCollection<NetworkItem> AllItemsReceived => GetReceivedItems();

        ReadOnlyCollection<NetworkItem> GetReceivedItems()
        {
            lock (itemQueueLockObject)
            {
                return new ReadOnlyCollection<NetworkItem>(allItemsReceived.ToArray());
            }
        }

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
            lock (itemQueueLockObject)
            {
                return itemQueue.Count > 0;
            }
        }

        /// <summary>
        ///     Peek the next item on the queue to be handled. 
        ///     The item will remain on the queue until dequeued with <see cref="DequeueItem"/>.
        /// </summary>
        /// <returns>
        ///     The next item to be handled as a <see cref="NetworkItem"/>.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        ///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
        /// </exception>
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
        /// <exception cref="T:System.InvalidOperationException">
        ///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
        /// </exception>
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
        /// <exception cref="T:System.InvalidOperationException">
        ///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
        /// </exception>
        public NetworkItem DequeueItem()
        {
            lock (itemQueueLockObject)
            {
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
        ///     The name of the item as a string, or null if no such item is found.
        /// </returns>
        public string GetItemName(int id)
        {
            if (itemLookupCache.TryGetValue(id, out var name))
            {
                return name;
            }
            else
            {
                if (!dataPackageCache.TryGetDataPackageFromCache(out var dataPackage))
                {
                    return null;
                }

                var gameDataContainingId = dataPackage.Games.SingleOrDefault(x => x.Value.ItemLookup.ContainsValue(id));

                if (string.IsNullOrEmpty(gameDataContainingId.Key) || gameDataContainingId.Value == null)
                {
                    return null;
                }

                var gameDataItemLookup = gameDataContainingId.Value.ItemLookup.ToDictionary(x => x.Value, x => x.Key);
                foreach (var kvp in gameDataItemLookup)
                {
                    itemLookupCache.Add(kvp.Key, kvp.Value);
                }

                return itemLookupCache.TryGetValue(id, out name)
                    ? name
                    : null;
            }
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

                    lock (itemQueueLockObject)
                    {
                        foreach (var item in receivedItemsPacket.Items)
                        {
                            if (!allItemsReceived.Contains(item))
                            {
                                allItemsReceived.Add(item);
                                itemQueue.Enqueue(item);

                                if (ItemReceived != null)
                                {
                                    ItemReceived(this);
                                }
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
                var previouslyReceived = new List<NetworkItem>(allItemsReceived);

                itemQueue.Clear();
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
}