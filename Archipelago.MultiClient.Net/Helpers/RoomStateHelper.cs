using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.ObjectModel;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class RoomStateHelper
    {
        private string[] tags;

        /// <summary>
        /// The amount of points it costs to receive a hint from the server.
        /// </summary>
        public int HintCost { get; private set;  }
        /// <summary>
        /// The amount of hint points you receive per item/location check completed.
        /// </summary>
        public int LocationCheckPoints { get; private set; }
        /// <summary>
        /// The client's current hint points.
        /// </summary>
        public int HintPoints { get; private set; }
        /// <summary>
        /// The version of Archipelago which the server is running.
        /// </summary>
        public Version Version { get; private set; }
        /// <summary>
        /// Denoted whether a password is required to join this room.
        /// </summary>
        public bool HasPassword { get; private set; }
        /// <summary>
        /// An enumeration containing the possible forfeit command permission.
        /// </summary>
        public Permissions ForfeitPermissions { get; private set; }
        /// <summary>
        /// An enumeration containing the possible collect command permission.
        /// </summary>
        public Permissions CollectPermissions { get; private set; }
        /// <summary>
        /// An enumeration containing the possible remaining command permission.
        /// </summary>
        public Permissions RemainingPermissions { get; private set; }
        /// <summary>
        /// Uniquely identifying name of this generation
        /// </summary>
        public string Seed { get; private set; }
        /// <summary>
        /// Time stamp of we first connected to the server, and the server send us the RoomInfoPacker.
        /// Send for time synchronization if wanted for things like the DeathLink Bounce.
        /// </summary>
        public DateTime RoomInfoSendTime { get; private set; }
        /// <summary>
        /// Denotes special features or capabilities that the server is capable of.
        /// </summary>
        public ReadOnlyCollection<string> ServerTags =>
            tags == null 
                ? default 
                : new ReadOnlyCollection<string>(tags);
        
        public RoomStateHelper(IArchipelagoSocketHelper socket)
        {
            socket.PacketReceived += PacketReceived;
        }

        private void PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case RoomUpdatePacket roomUpdatePacket:
	                OnRoomUpdatedPacketReceived(roomUpdatePacket);
	                break;
                case RoomInfoPacket roomInfoPacket:
                    OnRoomInfoPacketReceived(roomInfoPacket);
                    break;
            }
        }

        private void OnRoomInfoPacketReceived(RoomInfoPacket packet)
        {
            HintCost = packet.HintCost;
            LocationCheckPoints = packet.LocationCheckPoints;
            Version = packet.Version?.ToVersion();
            HasPassword = packet.Password;
            Seed = packet.SeedName;
            RoomInfoSendTime = UnixTimeConverter.UnixTimeStampToDateTime(packet.Timestamp);

            tags = packet.Tags;

            if (packet.Permissions != null)
            {
                if (packet.Permissions.TryGetValue("forfeit", out var forfeitPermissions))
                    ForfeitPermissions = forfeitPermissions;
                if (packet.Permissions.TryGetValue("collect", out var collectPermissions))
                    CollectPermissions = collectPermissions;
                if (packet.Permissions.TryGetValue("remaining", out var remainingPermissions))
                    RemainingPermissions = remainingPermissions;
            }
        }

        private void OnRoomUpdatedPacketReceived(RoomUpdatePacket packet)
        {
            if(packet.HintCost.HasValue)
                HintCost = packet.HintCost.Value;

            if (packet.LocationCheckPoints.HasValue)
                LocationCheckPoints = packet.LocationCheckPoints.Value;

            if (packet.HintPoints.HasValue)
                HintPoints = packet.HintPoints.Value;

            if (packet.Tags != null)
                tags = packet.Tags;

            if (packet.Password.HasValue)
                HasPassword = packet.Password.Value;

            if (packet.Permissions != null)
            {
                if (packet.Permissions.TryGetValue("forfeit", out var forfeitPermissions))
                    ForfeitPermissions = forfeitPermissions;

                if (packet.Permissions.TryGetValue("collect", out var collectPermissions))
                    CollectPermissions = collectPermissions;

                if (packet.Permissions.TryGetValue("remaining", out var remainingPermissions))
                    RemainingPermissions = remainingPermissions;
            }
        }
    }
}