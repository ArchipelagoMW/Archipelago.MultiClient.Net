using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.ObjectModel;

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Provides information about the current state of the server
	/// </summary>
	public interface IRoomStateHelper
	{
		/// <summary>
		/// The amount of points it costs to receive a hint from the server.
		/// </summary>
		int HintCost { get; }

		/// <summary>
		/// The percentage of total locations that need to be checked to receive a hint from the server.
		/// </summary>
		int HintCostPercentage { get; }

		/// <summary>
		/// The amount of hint points you receive per item/location check completed.
		/// </summary>
		int LocationCheckPoints { get; }

		/// <summary>
		/// The client's current hint points.
		/// </summary>
		int HintPoints { get; }

		/// <summary>
		/// The version of Archipelago which the server is running.
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Denoted whether a password is required to join this room.
		/// </summary>
		bool HasPassword { get; }

		/// <summary>
		/// An enumeration containing the possible release command permission.
		/// </summary>
		Permissions ReleasePermissions { get; }

		/// <summary>
		/// An enumeration containing the possible forfeit command permission. 
		/// Deprecated, use Release Permissions instead
		/// </summary>
		Permissions ForfeitPermissions { get; }

		/// <summary>
		/// An enumeration containing the possible collect command permission.
		/// </summary>
		Permissions CollectPermissions { get; }

		/// <summary>
		/// An enumeration containing the possible remaining command permission.
		/// </summary>
		Permissions RemainingPermissions { get; }

		/// <summary>
		/// Uniquely identifying name of this generation
		/// </summary>
		string Seed { get; }

		/// <summary>
		/// Time stamp of we first connected to the server, and the server send us the RoomInfoPacker.
		/// Send for time synchronization if wanted for things like the DeathLink Bounce.
		/// </summary>
		DateTime RoomInfoSendTime { get; }

		/// <summary>
		/// Denotes special features or capabilities that the server is capable of.
		/// </summary>
		ReadOnlyCollection<string> ServerTags { get; }
	}

	///<inheritdoc/>
	public class RoomStateHelper : IRoomStateHelper
	{
	    readonly ILocationCheckHelper locationCheckHelper;

	    string[] tags;

	    ///<inheritdoc/>
		public int HintCost { get; private set;  }
	    ///<inheritdoc/>
		public int HintCostPercentage { get; private set; }
	    ///<inheritdoc/>
		public int LocationCheckPoints { get; private set; }
	    ///<inheritdoc/>
		public int HintPoints { get; private set; }
	    ///<inheritdoc/>
		public Version Version { get; private set; }
	    ///<inheritdoc/>
		public bool HasPassword { get; private set; }
	    ///<inheritdoc/>
		public Permissions ReleasePermissions { get; private set; }
	    ///<inheritdoc/>
		public Permissions ForfeitPermissions => ReleasePermissions;
	    ///<inheritdoc/>
		public Permissions CollectPermissions { get; private set; }
	    ///<inheritdoc/>
		public Permissions RemainingPermissions { get; private set; }
	    ///<inheritdoc/>
		public string Seed { get; private set; }
	    ///<inheritdoc/>
		public DateTime RoomInfoSendTime { get; private set; }
	    ///<inheritdoc/>
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

			//TODO recalculate hint cost here as this is where locationCheckHelper.AllLocations is not 0 (hopefully depending on order)
			//should probably unit test this
			HintCost = (int)Math.Max(0m, locationCheckHelper.AllLocations.Count * 0.01m * HintCostPercentage);
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