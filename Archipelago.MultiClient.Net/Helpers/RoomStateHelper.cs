﻿using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.ObjectModel;

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Provides information about the current state of the server
	/// </summary>
	public class RoomStateHelper
    {
	    readonly ILocationCheckHelper locationCheckHelper;

	    string[] tags;

        /// <summary>
        /// The amount of points it costs to receive a hint from the server.
        /// </summary>
        public int HintCost { get; private set;  }
		/// <summary>
		/// The percentage of total locations that need to be checked to receive a hint from the server.
		/// </summary>
		public int HintCostPercentage { get; private set; }
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
        /// An enumeration containing the possible release command permission.
        /// </summary>
        public Permissions ReleasePermissions { get; private set; }
		/// <summary>
		/// An enumeration containing the possible forfeit command permission. 
		/// Deprecated, use Release Permissions instead
		/// </summary>
		public Permissions ForfeitPermissions => ReleasePermissions;
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
        
        internal RoomStateHelper(IArchipelagoSocketHelper socket, ILocationCheckHelper locationCheckHelper)
        {
	        this.locationCheckHelper = locationCheckHelper;

	        socket.PacketReceived += PacketReceived;
        }

        void PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
	            case ConnectedPacket connectedPacket:
		            OnConnectedPacketReceived(connectedPacket);
		            break;
                case RoomUpdatePacket roomUpdatePacket:
	                OnRoomUpdatedPacketReceived(roomUpdatePacket);
	                break;
                case RoomInfoPacket roomInfoPacket:
                    OnRoomInfoPacketReceived(roomInfoPacket);
                    break;
            }
        }

        void OnConnectedPacketReceived(ConnectedPacket packet)
        {
	        if (packet.HintPoints.HasValue)
		        HintPoints = packet.HintPoints.Value;
        }

        void OnRoomInfoPacketReceived(RoomInfoPacket packet)
        {
	        HintCostPercentage = packet.HintCostPercentage;
	        HintCost = (int)Math.Max(0m, locationCheckHelper.AllLocations.Count * 0.01m * packet.HintCostPercentage);
			LocationCheckPoints = packet.LocationCheckPoints;
            Version = packet.Version?.ToVersion();
            HasPassword = packet.Password;
            Seed = packet.SeedName;
            RoomInfoSendTime = UnixTimeConverter.UnixTimeStampToDateTime(packet.Timestamp);

            tags = packet.Tags;

            if (packet.Permissions == null) 
	            return;

            if (packet.Permissions.TryGetValue("release", out var releasePermissions))
	            ReleasePermissions = releasePermissions;
            else if (packet.Permissions.TryGetValue("forfeit", out var forfeitPermissions))
	            ReleasePermissions = forfeitPermissions;

            if (packet.Permissions.TryGetValue("collect", out var collectPermissions))
	            CollectPermissions = collectPermissions;
            if (packet.Permissions.TryGetValue("remaining", out var remainingPermissions))
	            RemainingPermissions = remainingPermissions;
        }

        void OnRoomUpdatedPacketReceived(RoomUpdatePacket packet)
        {
	        if (packet.HintCostPercentage.HasValue)
	        {
		        HintCostPercentage = packet.HintCostPercentage.Value;
		        HintCost = (int)Math.Max(0m, locationCheckHelper.AllLocations.Count * 0.01m * packet.HintCostPercentage.Value);
	        }

			if (packet.LocationCheckPoints.HasValue)
                LocationCheckPoints = packet.LocationCheckPoints.Value;

            if (packet.HintPoints.HasValue)
                HintPoints = packet.HintPoints.Value;

            if (packet.Tags != null)
                tags = packet.Tags;

            if (packet.Password.HasValue)
                HasPassword = packet.Password.Value;

            if (packet.Permissions == null) 
	            return;

            if (packet.Permissions.TryGetValue("release", out var releasePermissions))
	            ReleasePermissions = releasePermissions;
            else if (packet.Permissions.TryGetValue("forfeit", out var forfeitPermissions))
	            ReleasePermissions = forfeitPermissions;

            if (packet.Permissions.TryGetValue("collect", out var collectPermissions))
	            CollectPermissions = collectPermissions;

            if (packet.Permissions.TryGetValue("remaining", out var remainingPermissions))
	            RemainingPermissions = remainingPermissions;
        }
    }
}