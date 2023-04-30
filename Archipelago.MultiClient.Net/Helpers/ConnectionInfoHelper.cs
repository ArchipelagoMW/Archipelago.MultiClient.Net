using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System;

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Provides information about your current connection
	/// </summary>
	interface IConnectionInfoProvider
    {
        /// <summary>
        /// The game you are connected to or an empty string otherwise
        /// </summary>
        string Game { get; }
        /// <summary>
        /// The team you are connected to or -1 otherwise
        /// </summary>
        int Team { get; }
        /// <summary>
        /// The slot you are connected to or -1 otherwise
        /// </summary>
        int Slot { get; }
        /// <summary>
        /// The tags under which you client is connected or an empty array otherwise
        /// </summary>
        string[] Tags { get; }
        /// <summary>
        /// The Item Handling Flags which your connection is currently using, defaults to NoItems
        /// </summary>
        ItemsHandlingFlags ItemsHandlingFlags { get; }
        /// <summary>
        /// The Uniquely Identifiable string under which you client is connected or an empty string otherwise
        /// </summary>
        string Uuid { get; }

        /// <summary>
        /// Send a ConnectUpdate packet and set the tags and ItemsHandlingFlags for the current connection to the provided params.
        /// </summary>
        /// <param name="tags">New tags for the current connection.</param>
        void UpdateConnectionOptions(string[] tags);

        /// <summary>
        /// Send a ConnectUpdate packet and set the tags and ItemsHandlingFlags for the current connection to the provided params.
        /// </summary>
        /// <param name="itemsHandlingFlags">New ItemsHandlingFlags for the current connection.</param>
        void UpdateConnectionOptions(ItemsHandlingFlags itemsHandlingFlags);

        /// <summary>
        /// Send a ConnectUpdate packet and set the tags and ItemsHandlingFlags for the current connection to the provided params.
        /// </summary>
        /// <param name="tags">New tags for the current connection.</param>
        /// <param name="itemsHandlingFlags">New ItemsHandlingFlags for the current connection.</param>
        void UpdateConnectionOptions(string[] tags, ItemsHandlingFlags itemsHandlingFlags);
    }
	
	///<inheritdoc/>
	public class ConnectionInfoHelper : IConnectionInfoProvider
    {
        readonly IArchipelagoSocketHelper socket;

        ///<inheritdoc/>
		public string Game { get; private set; }
        ///<inheritdoc/>
		public int Team { get; private set; }
        ///<inheritdoc/>
		public int Slot { get; private set; }
        ///<inheritdoc/>
		public string[] Tags { get; internal set; }
        ///<inheritdoc/>
		public ItemsHandlingFlags ItemsHandlingFlags { get; internal set; }
        ///<inheritdoc/>
		public string Uuid { get; private set; }

		internal ConnectionInfoHelper(IArchipelagoSocketHelper socket)
        {
            this.socket = socket;

            Reset();

            socket.PacketReceived += PacketReceived;
        }

        void PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket connectedPacket:
                    Team = connectedPacket.Team;
                    Slot = connectedPacket.Slot;
	                break;
                case ConnectionRefusedPacket _:
                    Reset();
                    break;
            }
        }

        internal void SetConnectionParameters(string game, string[] tags, ItemsHandlingFlags itemsHandlingFlags, string uuid)
        {
            Game = game;
            Tags = tags ?? new string[0];
            ItemsHandlingFlags = itemsHandlingFlags;
            Uuid = uuid ?? Guid.NewGuid().ToString();
        }

        void Reset()
        {
            Game = null;
            Team = -1;
            Slot = -1;
            Tags = new string[0];
            ItemsHandlingFlags = ItemsHandlingFlags.NoItems;
            Uuid = null;
        }

        ///<inheritdoc/>
		public void UpdateConnectionOptions(string[] tags) => UpdateConnectionOptions(tags, ItemsHandlingFlags);

        ///<inheritdoc/>
		public void UpdateConnectionOptions(ItemsHandlingFlags itemsHandlingFlags) => UpdateConnectionOptions(Tags, ItemsHandlingFlags);

        ///<inheritdoc/>
		public void UpdateConnectionOptions(string[] tags, ItemsHandlingFlags itemsHandlingFlags)
        {
            SetConnectionParameters(Game, tags, itemsHandlingFlags, Uuid);

            socket.SendPacket(new ConnectUpdatePacket
            {
                Tags = Tags,
                ItemsHandling = ItemsHandlingFlags
            });
        }
    }
}