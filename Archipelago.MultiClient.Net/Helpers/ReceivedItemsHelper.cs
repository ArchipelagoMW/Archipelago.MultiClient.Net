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
        private readonly ArchipelagoSocketHelper session;
        private readonly IDataPackageCache dataPackageCache;
        private DataPackage dataPackage;
        private int itemsReceivedIndex = 0;
        private Queue<NetworkItem> itemQueue = new Queue<NetworkItem>();
        private List<NetworkItem> allItemsReceived = new List<NetworkItem>();
        private Dictionary<int, string> itemLookupCache = new Dictionary<int, string>();
        private object itemQueueLockObject;

        public int Index => itemsReceivedIndex;
        public ReadOnlyCollection<NetworkItem> AllItemsReceived => new ReadOnlyCollection<NetworkItem>(allItemsReceived);

        public delegate void ItemReceivedHandler();
        public event ItemReceivedHandler ItemReceived;

        internal ReceivedItemsHelper(ArchipelagoSocketHelper session, IDataPackageCache dataPackageCache)
        {
            this.session = session;
            this.dataPackageCache = dataPackageCache;
            session.PacketReceived += Session_PacketReceived;
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
        /// <param name="game">
        ///     The game for which to look up the item id. This lookup is derived from the DataPackage packet.
        /// </param>
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
            if (!VerifyDataPackageReceived())
            {
                // Need to wait until the DataPackage comes back.
                AwaitDataPackage();
            }

            if (itemLookupCache.TryGetValue(id, out var name))
            {
                return name;
            }
            else
            {
                var gameDataContainingId = dataPackage.Games.Where(x => x.Value.ItemLookup.ContainsValue(id)).Single();
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
        }

        /// <remarks>
        ///     I don't really have an asynchronous choice here so this is what we get.
        ///     I'm open to ideas/advice.
        /// </remarks>
        private void AwaitDataPackage()
        {
            while(!dataPackageCache.TryGetDataPackageFromCache(out var _))
            {

            }
            dataPackageCache.TryGetDataPackageFromCache(out dataPackage);
        }

        private bool VerifyDataPackageReceived()
        {
            if (!dataPackageCache.TryGetDataPackageFromCache(out var _))
            {
                session.SendPacket(new GetDataPackagePacket());
                return false;
            }

            return true;
        }

        private void Session_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.ReceivedItems:
                    {
                        var receivedItemsPacket = (ReceivedItemsPacket)packet;

                        if (itemsReceivedIndex != receivedItemsPacket.Index)
                        {
                            session.SendPacket(new SyncPacket());
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
