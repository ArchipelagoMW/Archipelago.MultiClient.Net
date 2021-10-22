using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class ReceivedItemsHelper
    {
        private readonly ArchipelagoSocketHelper session;
        private DataPackage dataPackage;
        private int itemsReceivedIndex = 0;
        private Queue<NetworkItem> itemQueue = new Queue<NetworkItem>();
        private List<NetworkItem> allItemsReceived = new List<NetworkItem>();
        private Dictionary<string, Dictionary<int, string>> itemLookupCache = new Dictionary<string, Dictionary<int, string>>();

        public int Index => itemsReceivedIndex;
        public ReadOnlyCollection<NetworkItem> AllItemsReceived => new ReadOnlyCollection<NetworkItem>(allItemsReceived);

        public delegate void ItemReceivedHandler();
        public event ItemReceivedHandler ItemReceived;

        public ReceivedItemsHelper(ArchipelagoSocketHelper session)
        {
            this.session = session;

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
            return itemQueue.Peek();
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
        public string PeekItemName(string game)
        {
            var item = itemQueue.Peek();

            return GetItemName(item.Item, game);
        }

        /// <summary>
        ///     Dequeues and returns the next item on the queue to be handled.
        /// </summary>
        /// <returns>
        ///     The next item to be handled as a <see cref="NetworkItem"/>.
        /// </returns>
        public NetworkItem DequeueItem()
        {
            itemsReceivedIndex++;
            return itemQueue.Dequeue();
        }

        /// <summary>
        ///     Perform a lookup using the DataPackage sent as a source of truth to lookup a particular item id for a particular game.
        /// </summary>
        /// <param name="id">
        ///     Id of the item to lookup.
        /// </param>
        /// <param name="game">
        ///     Name of the game to lookup the item for.
        /// </param>
        /// <returns>
        ///     The name of the item as a string.
        /// </returns>
        public string GetItemName(int id, string game)
        {
            if (!VerifyDataPackageReceived())
            {
                // Need to wait until the DataPackage comes back.
                AwaitDataPackage();
            }

            if (dataPackage.Games.TryGetValue(game, out var gameData))
            {
                if (itemLookupCache.TryGetValue(game, out var cachedLookup))
                {
                    return PerformItemIdLookup(cachedLookup, id, game);
                }
                else
                {
                    var itemLookupTransposed = dataPackage.Games[game].ItemLookup.ToDictionary(x => x.Value, x => x.Key);
                    itemLookupCache.Add(game, itemLookupTransposed);

                    return PerformItemIdLookup(itemLookupTransposed, id, game);
                }
            }
            else
            {
                throw new UnknownGameException($"Attempt to look up GameData for game `{game}` failed. `{game}` was not present in data package.");
            }
        }

        /// <remarks>
        ///     I don't really have an asynchronous choice here so this is what we get.
        ///     I'm open to ideas/advice.
        /// </remarks>
        private void AwaitDataPackage()
        {
            while(dataPackage == null)
            {

            }
        }

        private bool VerifyDataPackageReceived()
        {
            bool isNull;
            if (isNull = dataPackage == null)
            {
                session.SendPacket(new GetDataPackagePacket());
            }

            return !isNull;
        }

        private string PerformItemIdLookup(Dictionary<int, string> lookup, int id, string game)
        {
            if (lookup.TryGetValue(id, out var name))
            {
                return name;
            }
            else
            {
                throw new UnknownItemIdException($"Attempt to look up item id `{id}` for game `{game}` failed.");
            }
        }

        private void Session_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.DataPackage:
                    {
                        var dataPackagePacket = (DataPackagePacket)packet;

                        dataPackage = dataPackagePacket.DataPackage;
                        break;
                    }

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
                        break;
                    }
            }
        }

        private void PerformResynchronization(ReceivedItemsPacket receivedItemsPacket)
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
