using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System;

namespace Archipelago.MultiClient.Net.Helpers
{
    internal interface IConnectionInfoProvider
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
    }

    public class ConnectionInfoHelper : IConnectionInfoProvider
    {
        public string Game { get; private set; }
        public int Team { get; private set; }
        public int Slot { get; private set; }
        public string[] Tags { get; internal set; }
        public ItemsHandlingFlags ItemsHandlingFlags { get; internal set; }
        public string Uuid { get; private set; }

        public ConnectionInfoHelper(IArchipelagoSocketHelper socket)
        {
            Reset();

            socket.PacketReceived += PacketReceived;
        }

        private void PacketReceived(ArchipelagoPacketBase packet)
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

        private void Reset()
        {
            Game = "";
            Team = -1;
            Slot = -1;
            Tags = new string[0];
            ItemsHandlingFlags = ItemsHandlingFlags.NoItems;
            Uuid = "";
        }
    }
}