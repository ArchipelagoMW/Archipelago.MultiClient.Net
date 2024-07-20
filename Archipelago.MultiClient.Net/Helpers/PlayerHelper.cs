using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// A helper class containing information about all players in the current multiworld
	/// </summary>
	public interface IPlayerHelper
    {
		/// <summary>
		/// A dictionary of all team's containing all their PlayerInfo's index by their slot
		/// </summary>
#if NET35 || NET40
		Dictionary<int, ReadOnlyCollection<PlayerInfo>> Players { get; }
#else
		ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>> Players { get; }
#endif

	    /// <summary>
	    /// A enumerable of PlayerInfo's for all players in the multiworld
	    /// </summary>
		IEnumerable<PlayerInfo> AllPlayers { get; }

		/// <summary>
		/// The player info for the currently connected player
		/// </summary>
		PlayerInfo ActivePlayer { get; }

		/// <summary>
		/// Returns the Alias corresponding to the provided player slot
		/// Alias defaults to the player's name until a different alias is specifically set
		/// </summary>
		/// <param name="slot">The slot of which to retrieve the alias</param>
		/// <returns>The player's alias, or null if no such player is found</returns>
		string GetPlayerAlias(int slot);

        /// <summary>
        /// Returns the Name corresponding to the provided player slot
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the name</param>
        /// <returns>The player's name, or null if no such player is found</returns>
        string GetPlayerName(int slot);

        /// <summary>
        /// Returns the Alias and Name corresponding to the provided player slot
        /// Alias defaults to the player's name until a different alias is specifically set
        /// The result is returned in the format of "Alias (Name)"
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the alias</param>
        /// <returns>The player's alias and name in the following format of "Alias (Name)", or null if no such player is found</returns>
        string GetPlayerAliasAndName(int slot);

		/// <summary>
		/// Gets the PlayerInfo for the provided team and slot, or null if no such team / slot exists
		/// </summary>
		/// <param name="team">the team to lookup the slot in</param>
		/// <param name="slot">the slot to lookup</param>
		/// <returns>the playerinfo of corresponding slot, or null if no such slot exists</returns>
		PlayerInfo GetPlayerInfo(int team, int slot);

		/// <summary>
		/// Gets the PlayerInfo for the provided team and slot, or null if no such slot exists
		/// </summary>
		/// <param name="slot">the slot to lookup</param>
		/// <returns>the playerinfo of corresponding slot, or null if no such slot exists</returns>
		PlayerInfo GetPlayerInfo(int slot);
	}

	/// <inheritdoc/>
    public class PlayerHelper : IPlayerHelper
    {
	    readonly IConnectionInfoProvider connectionInfo;

#if NET35 || NET40
	    Dictionary<int, ReadOnlyCollection<PlayerInfo>> players = new Dictionary<int, ReadOnlyCollection<PlayerInfo>>(0);
#else
	    ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>> players = 
		    new ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>>(new Dictionary<int, ReadOnlyCollection<PlayerInfo>>(0));
#endif

	    /// <inheritdoc/>
#if NET35 || NET40
		public Dictionary<int, ReadOnlyCollection<PlayerInfo>> Players => players;
#else
		public ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>> Players => players;
#endif

	    /// <inheritdoc/>
		public IEnumerable<PlayerInfo> AllPlayers => players.SelectMany(kvp => kvp.Value);

		/// <inheritdoc/>
		public PlayerInfo ActivePlayer => GetPlayerInfo(connectionInfo.Team, connectionInfo.Slot);

	    /// <inheritdoc/>
	    public PlayerInfo GetPlayerInfo(int team, int slot) =>
		    players.Count > team && players[team].Count > slot
			    ? players[team][slot]
			    : null;

	    /// <inheritdoc/>
		public PlayerInfo GetPlayerInfo(int slot) =>
		    GetPlayerInfo(connectionInfo.Team, slot);

		internal PlayerHelper(IArchipelagoSocketHelper socket, IConnectionInfoProvider connectionInfo)
        {
	        this.connectionInfo = connectionInfo;

	        socket.PacketReceived += PacketReceived;
        }

		/// <inheritdoc/>
		public string GetPlayerAlias(int slot)
        {
            if (players == null)
                return null;

            var playerInfo = players[connectionInfo.Team].FirstOrDefault(p => p.Slot == slot);

            return playerInfo?.Alias;
        }

		/// <inheritdoc/>
		public string GetPlayerName(int slot)
        {
            if (players == null)
                return null;

            var playerInfo = players[connectionInfo.Team].FirstOrDefault(p => p.Slot == slot);

            return playerInfo?.Name;
        }

		/// <inheritdoc/>
		public string GetPlayerAliasAndName(int slot)
        {
            if (players == null)
                return null;

            var playerInfo = players[connectionInfo.Team].FirstOrDefault(p => p.Slot == slot);
            if (playerInfo == null)
                return null;

            return $"{playerInfo.Alias} ({playerInfo.Name})";
        }

		void PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket connectedPacket:
                    CreatePlayerInfo(connectedPacket.Players, connectedPacket.SlotInfo);
                    break;
                case RoomUpdatePacket roomUpdatePacket:
                    UpdatePlayerInfo(roomUpdatePacket.Players);
	                break;
            }
        }

        void CreatePlayerInfo(NetworkPlayer[] networkPlayers, Dictionary<int, NetworkSlot> slotInfos)
        {
	        var groups = slotInfos == null 
	            ? new NetworkSlot[0] 
	            : slotInfos.Values.Where(s => s.Type == SlotType.Group).ToArray();

            var maxTeam = 0;
			var maxSlot = 0;

            foreach (var p in networkPlayers)
            {
	            if (p.Team > maxTeam)
		            maxTeam = p.Team;
	            if (p.Slot > maxSlot)
		            maxSlot = p.Slot;
			}

			var playerData = new Dictionary<int, PlayerInfo[]>(maxTeam);

			//team is 0 based
			for (var i = 0; i <= maxTeam; i++)
            {
	            //slot 0 is for server, player slots start at 1
				playerData[i] = new PlayerInfo[maxSlot + 1];
	            playerData[i][0] = new PlayerInfo
	            {
		            Team = i,
		            Slot = 0,
		            Name = "Server",
		            Alias = "Server",
		            Game = "Archipelago",
		            Groups = new NetworkSlot[0]
	            };
			}

			foreach (var p in networkPlayers)
            {
				playerData[p.Team][p.Slot] = new PlayerInfo {
					Team = p.Team,
					Slot = p.Slot,
					Name = p.Name,
					Alias = p.Alias,
					Game = slotInfos?[p.Slot].Game,
					Groups = groups.Where(g => g.GroupMembers.Contains(p.Slot)).ToArray(),
					GroupMembers = slotInfos?[p.Slot].GroupMembers
				};
            }

			var allPlayers = new Dictionary<int, ReadOnlyCollection<PlayerInfo>>(playerData.Count);
			foreach (var kvp in playerData)
				allPlayers[kvp.Key] = new ReadOnlyCollection<PlayerInfo>(kvp.Value);

#if NET35 || NET40
	        players = allPlayers;
#else
	        players = new ReadOnlyDictionary<int, ReadOnlyCollection<PlayerInfo>>(allPlayers);
#endif
		}

		void UpdatePlayerInfo(NetworkPlayer[] networkPlayers)
		{
			if (networkPlayers == null || networkPlayers.Length <= 0) 
				return;

			foreach (var p in networkPlayers)
			{
				players[p.Team][p.Slot].Name = p.Name;
				players[p.Team][p.Slot].Alias = p.Alias;
			}
		}
    }

    /// <summary>
    /// Information about a specific player
    /// </summary>
    public class PlayerInfo
    {
        /// <summary>
        /// The team of this player
        /// </summary>
        public int Team { get; internal set; }
        /// <summary>
        /// The slot of this player
        /// </summary>
        public int Slot { get; internal set; }
        /// <summary>
        /// A custom name Alias for this player, that can optionally set with !alias, if no alias is set will return Name instead
        /// </summary>
        public string Alias { get; internal set; }
        /// <summary>
        /// The name of that player
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// The game the player is playing
        /// </summary>
        public string Game { get; internal set; }
        /// <summary>
        /// A array of groups this player is part of
        /// </summary>
        public NetworkSlot[] Groups { get; internal set; }
		/// <summary>
		/// If the slot is a group, an array of its member slots
		/// </summary>
		internal int[] GroupMembers { get; set; }

		/// <summary>
		/// Checks if the provided team and slot, are sharing any slot groups with this player
		/// </summary>
		/// <param name="team">The team to check</param>
		/// <param name="slot">The slot to check</param>
		/// <returns>Whether this player has any itemlinks in common with the specified player</returns>
		[Obsolete]
        public bool IsSharingGroupWith(int team, int slot) => 
			Team == team && Slot == slot || (Groups != null && Groups.Any(g => g.GroupMembers.Contains(slot)));

		/// <summary>
		/// If this player is a group, gets its members. Otherwise returns null.
		/// </summary>
		public IEnumerable<PlayerInfo> GetGroupMembers(IPlayerHelper playerHelper)
		{
			if (GroupMembers == null || GroupMembers.Length == 0)
			{
				return null;
			}
			return GroupMembers.Select(g => playerHelper.GetPlayerInfo(Team, g));
		}

		/// <summary>
		/// Converts the PlayerInfo to the slot
		/// </summary>
		public static implicit operator int(PlayerInfo p) => p.Slot;

		/// <inheritdoc/>
		public static bool operator ==(PlayerInfo left, PlayerInfo right)
		{
			return EqualityComparer<PlayerInfo>.Default.Equals(left, right);
		}

		/// <inheritdoc/>
		public static bool operator !=(PlayerInfo left, PlayerInfo right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is PlayerInfo info && Team == info.Team && Slot == info.Slot;

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			var hashCode = -1713739875;
			hashCode = hashCode * -1521134295 + Team.GetHashCode();
			hashCode = hashCode * -1521134295 + Slot.GetHashCode();
			return hashCode;
		}

		/// <summary>
		/// Returns the Alias of the player
		/// </summary>
		public override string ToString() => Alias ?? Name ?? $"Player: {Slot}";

		/// <summary>
		/// Creates and Empty PlayerInfo object, should probably not even be exposed
		/// </summary>
		public PlayerInfo() {}

		/// <summary>
		/// Creates and PlayerInfo object, used by json deserialization
		/// </summary>
		/// <param name="team">The team of this player</param>
		/// <param name="slot">The slot of this player</param>
		/// <param name="name">The name of that player</param>
		/// <param name="alias">The alias of that player</param>
		/// <param name="game">The game the player is playing</param>
		/// <param name="groups">A array of groups this player is part of</param>
		[JsonConstructor]
		public PlayerInfo(int team, int slot, string name, string alias, string game, NetworkSlot[] groups, int[] groupMembers)
		{
			Team = team;
			Slot = slot;
			Name = name;
			Alias = alias;
			Game = game;
			Groups = groups;
			GroupMembers = groupMembers;
		}
	}
}