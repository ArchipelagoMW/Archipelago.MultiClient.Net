using Archipelago.MultiClient.Net.ConcurrentCollection;
using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.ObjectModel;

#if !NET35
using System.Collections.Concurrent;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Provides simplified methods to receive items
	/// </summary>
	public interface IReceivedItemsHelper
    {
		/// <summary>
		///     Perform a lookup using the DataPackage sent as a source of truth to lookup a particular item id for a particular game.
		/// </summary>
		/// <param name="id">
		///     Id of the item to lookup.
		/// </param>
		/// <param name="game">
		///     The game to lookup the item id for, if null will look in the game the local player is connected to.
		///		Negative item ids are always looked up under the Archipelago game
		/// </param>
		/// <returns>
		///     The name of the item as a string, or null if no such item is found.
		/// </returns>
		string GetItemName(long id, string game = null);

		/// <summary>
		/// Total number of items received
		/// </summary>
		int Index { get; }

		/// <summary>
		/// Full list of all items received
		/// </summary>
		ReadOnlyCollection<NetworkItem> AllItemsReceived { get; }

		/// <summary>
		///     Event triggered when an item is received. fires once for each item received
		/// </summary>
		event ReceivedItemsHelper.ItemReceivedHandler ItemReceived;

		/// <summary>
		///     Check whether there are any items in the queue. 
		/// </summary>
		/// <returns>
		///     True if the queue is not empty, otherwise false.
		/// </returns>
		bool Any();

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
		NetworkItem PeekItem();

		/// <summary>
		///     Peek the name of next item on the queue to be handled.
		/// </summary>
		/// <returns>
		///     The name of the item as a string, or null if no such item is found.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
		/// </exception>
		string PeekItemName();

		/// <summary>
		///     Dequeues and returns the next item on the queue to be handled.
		/// </summary>
		/// <returns>
		///     The next item to be handled as a <see cref="NetworkItem"/>.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
		/// </exception>
		NetworkItem DequeueItem();
    }

	/// <inheritdoc/>
	public class ReceivedItemsHelper : IReceivedItemsHelper
    {
        readonly IArchipelagoSocketHelper socket;
        readonly ILocationCheckHelper locationsHelper;
        readonly IDataPackageCache dataPackageCache;
        readonly IConnectionInfoProvider connectionInfoProvider;

        ConcurrentQueue<NetworkItem> itemQueue; 

        readonly IConcurrentList<NetworkItem> allItemsReceived;

        ReadOnlyCollection<NetworkItem> cachedReceivedItems;
		
		/// <inheritdoc/>
		public int Index => cachedReceivedItems.Count;
		/// <inheritdoc/>
		public ReadOnlyCollection<NetworkItem> AllItemsReceived => cachedReceivedItems;

		/// <inheritdoc/>
		public delegate void ItemReceivedHandler(ReceivedItemsHelper helper);
		/// <inheritdoc/>
		public event ItemReceivedHandler ItemReceived;

        internal ReceivedItemsHelper(
	        IArchipelagoSocketHelper socket, ILocationCheckHelper locationsHelper, 
	        IDataPackageCache dataPackageCache, IConnectionInfoProvider connectionInfoProvider)
        {
            this.socket = socket;
            this.locationsHelper = locationsHelper;
            this.dataPackageCache = dataPackageCache;
            this.connectionInfoProvider = connectionInfoProvider;

            itemQueue = new ConcurrentQueue<NetworkItem>();
			allItemsReceived = new ConcurrentList<NetworkItem>();
			cachedReceivedItems = allItemsReceived.AsReadOnlyCollection();

			socket.PacketReceived += Socket_PacketReceived;
        }

        /// <inheritdoc/>
		public bool Any() => !itemQueue.IsEmpty;

        /// <inheritdoc/>
		public NetworkItem PeekItem()
        {
            itemQueue.TryPeek(out var item);
            return item;
        }

        /// <inheritdoc/>
		public string PeekItemName()
        {
            itemQueue.TryPeek(out var item);
            return GetItemName(item.Item);
        }

        /// <inheritdoc/>
		public NetworkItem DequeueItem()
        {
            itemQueue.TryDequeue(out var item);
            return item;
        }

		/// <inheritdoc/>
        public string GetItemName(long id, string game = null)
		{
			if (game == null)
				game = connectionInfoProvider.Game ?? "Archipelago";

			if (id < 0)
				game = "Archipelago";

			if (!dataPackageCache.TryGetGameDataFromCache(game, out var dataPackage))
                return null;

            return dataPackage.Items.TryGetValue(id, out var itemName)
				? itemName
				: null;
        }

        void Socket_PacketReceived(ArchipelagoPacketBase packet)
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

                        cachedReceivedItems = allItemsReceived.AsReadOnlyCollection();

                        ItemReceived?.Invoke(this);
                    }
                    break;
                }
            }
        }

        void PerformResynchronization(ReceivedItemsPacket receivedItemsPacket)
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

                cachedReceivedItems = allItemsReceived.AsReadOnlyCollection();

				if (ItemReceived != null && !previouslyReceived.Contains(item))
                    ItemReceived(this);
            }
        }
    }
}
