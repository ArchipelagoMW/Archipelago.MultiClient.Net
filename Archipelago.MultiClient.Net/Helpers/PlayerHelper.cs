using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class PlayerHelper
    {
        private PlayerInfo[] players;

        /// <summary>
        /// A collection of PlayerInfo's where the index is the player their slot
        /// </summary>
        public ReadOnlyCollection<PlayerInfo> AllPlayers => new ReadOnlyCollection<PlayerInfo>(players);

        public PlayerHelper(ArchipelagoSocketHelper socket)
        {
            socket.PacketReceived += PacketReceived;
        }

        /// <summary>
        /// Returns the Alias corresponding to the provided player slot
        /// Alias defaults to the player's name until a different alias is specifically set
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the alias</param>
        /// <returns>The player's alias</returns>
        public string GetPlayerAlias(int slot)
        {
            if (players == null || slot >= players.Length || players[slot].Alias == null)
            {
                return $"Slot: {slot}";
            }

            return players[slot].Alias;
        }

        /// <summary>
        /// Returns the Name corresponding to the provided player slot
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the name</param>
        /// <returns>The player's name</returns>
        public string GetPlayerName(int slot)
        {
            if (players == null || slot >= players.Length || players[slot].Name == null)
            {
                return $"Slot: {slot}";
            }

            return players[slot].Name;
        }

        /// <summary>
        /// Returns the Alias and Name corresponding to the provided player slot
        /// Alias defaults to the player's name until a different alias is specifically set
        /// The result is returned in the format of "Alias (Name)"
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the alias</param>
        /// <returns>The player's alias and name in the following format of "Alias (Name)"</returns>
        public string GetPlayerAliasAndName(int slot)
        {
            if (players == null || slot >= players.Length || players[slot].Name == null)
            {
                return $"Slot: {slot}";
            }

            return $"{players[slot].Alias} ({players[slot].Name})";
        }

        private void PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket connectedPacket:
                    OnConnectedPacketReceived(connectedPacket);
                    break;
                case RoomInfoPacket roomInfoPacket:
                    OnRoomInfoPacketReceived(roomInfoPacket);
                    break;
            }
        }

        private void OnConnectedPacketReceived(ConnectedPacket packet)
        {
            UpdatePlayerInfo(packet.Players);
        }

        private void OnRoomInfoPacketReceived(RoomInfoPacket packet)
        {
            UpdateGames(packet.Games);
            UpdatePlayerInfo(packet.Players);
        }

        private void UpdateGames(List<string> games)
        {
            if (games != null && games.Count > 0)
            {
                if (players == null)
                {
                    players = games.Select(g => new PlayerInfo { Game = g }).ToArray();
                }
                else
                {
                    for (int i = 0; i < games.Count; i++)
                    {
                        players[i].Game = games[i];
                    }
                }
            }
        }

        private void UpdatePlayerInfo(List<NetworkPlayer> networkPlayers)
        {
            if (networkPlayers != null && networkPlayers.Count > 0)
            {
                players = new PlayerInfo[networkPlayers.Count];
                for (int i = 0; i < networkPlayers.Count; i++)
                {
                    players[i] = new PlayerInfo();
                    players[i].Team = networkPlayers[i].Team;
                    players[i].Slot = networkPlayers[i].Slot;
                    players[i].Name = networkPlayers[i].Name;
                    players[i].Alias = networkPlayers[i].Alias;
                }
            }
        }
    }

    public class PlayerInfo
    {
        public int Team { get; internal set; }
        public int Slot { get; internal set; }
        public string Alias { get; internal set; }
        public string Name { get; internal set; }
        public string Game { get; internal set; }
    }
}