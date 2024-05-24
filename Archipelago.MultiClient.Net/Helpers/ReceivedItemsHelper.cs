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
		ReadOnlyCollection<ItemInfo> AllItemsReceived { get; }

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
		ItemInfo PeekItem();

		/// <summary>
		///     Dequeues and returns the next item on the queue to be handled.
		/// </summary>
		/// <returns>
		///     The next item to be handled as a <see cref="NetworkItem"/>.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		///     The <see cref="T:System.Collections.Generic.Queue`1" /> is empty.
		/// </exception>
		ItemInfo DequeueItem();
    }

	/// <inheritdoc/>
	public class ReceivedItemsHelper : IReceivedItemsHelper
    {
        readonly IArchipelagoSocketHelper socket;
        readonly ILocationCheckHelper locationsHelper;
        readonly IItemInfoResolver itemInfoResolver;
        readonly IConnectionInfoProvider connectionInfoProvider;
        readonly IPlayerHelper playerHelper;

		ConcurrentQueue<ItemInfo> itemQueue; 

        readonly IConcurrentList<ItemInfo> allItemsReceived;

        ReadOnlyCollection<ItemInfo> cachedReceivedItems;
		
		/// <inheritdoc/>
		public int Index => cachedReceivedItems.Count;
		/// <inheritdoc/>
		public ReadOnlyCollection<ItemInfo> AllItemsReceived => cachedReceivedItems;

		/// <inheritdoc/>
		public delegate void ItemReceivedHandler(ReceivedItemsHelper helper);
		/// <inheritdoc/>
		public event ItemReceivedHandler ItemReceived;

        internal ReceivedItemsHelper(
	        IArchipelagoSocketHelper socket, ILocationCheckHelper locationsHelper,
	        IItemInfoResolver itemInfoResolver, IConnectionInfoProvider connectionInfoProvider,
			IPlayerHelper playerHelper)
        {
            this.socket = socket;
            this.locationsHelper = locationsHelper;
            this.itemInfoResolver = itemInfoResolver;
            this.connectionInfoProvider = connectionInfoProvider;
			this.playerHelper = playerHelper;

            itemQueue = new ConcurrentQueue<ItemInfo>();
			allItemsReceived = new ConcurrentList<ItemInfo>();
			cachedReceivedItems = allItemsReceived.AsReadOnlyCollection();

			socket.PacketReceived += Socket_PacketReceived;
        }

        /// <inheritdoc/>
		public bool Any() => !itemQueue.IsEmpty;

        /// <inheritdoc/>
		public ItemInfo PeekItem()
        {
            itemQueue.TryPeek(out var item);
            return item;
        }

        /// <inheritdoc/>
		public ItemInfo DequeueItem()
        {
            itemQueue.TryDequeue(out var item);
            return item;
        }

		/// <inheritdoc/>
        public string GetItemName(long id, string game = null) => itemInfoResolver.GetItemName(id, game);

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

                    foreach (var networkItem in receivedItemsPacket.Items)
                    {
	                    var playerForItem = playerHelper.Players[connectionInfoProvider.Team][networkItem.Player];
						var item = new ItemInfo(networkItem, connectionInfoProvider.Game, itemInfoResolver, playerForItem);
						
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
            itemQueue = new ConcurrentQueue<ItemInfo>();
#endif
			allItemsReceived.Clear();

            foreach (var networkItem in receivedItemsPacket.Items)
            {
	            var playerForItem = playerHelper.Players[connectionInfoProvider.Team][networkItem.Player];
	            var item = new ItemInfo(networkItem, connectionInfoProvider.Game, itemInfoResolver, playerForItem);

				itemQueue.Enqueue(item);
                allItemsReceived.Add(item);

                cachedReceivedItems = allItemsReceived.AsReadOnlyCollection();

				if (ItemReceived != null && !previouslyReceived.Contains(item))
                    ItemReceived(this);
            }
        }
    }
}
