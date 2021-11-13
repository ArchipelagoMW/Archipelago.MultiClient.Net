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
            if (players == null)
            {
                return $"Slot: {slot}";
            }

            var playerInfo = players.FirstOrDefault(p => p.Slot == slot);
            if (playerInfo == null)
            {
                return $"Slot: {slot}";
            }

            return playerInfo.Alias;
        }

        /// <summary>
        /// Returns the Name corresponding to the provided player slot
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the name</param>
        /// <returns>The player's name</returns>
        public string GetPlayerName(int slot)
        {
            if (players == null)
            {
                return $"Slot: {slot}";
            }

            var playerInfo = players.FirstOrDefault(p => p.Slot == slot);
            if (playerInfo == null)
            {
                return $"Slot: {slot}";
            }

            return playerInfo.Name;
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
            if (players == null)
            {
                return $"Slot: {slot}";
            }

            var playerInfo = players.FirstOrDefault(p => p.Slot == slot);
            if (playerInfo == null)
            {
                return $"Slot: {slot}";
            }

            return $"{playerInfo.Alias} ({playerInfo.Name})";
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
            UpdatePlayerInfo(packet.Players);
            UpdateGames(packet.Games);
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
                players = networkPlayers.Select(p => new PlayerInfo
                {
                    Team = p.Team,
                    Slot = p.Slot,
                    Name = p.Name,
                    Alias = p.Alias
                }).ToArray();
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