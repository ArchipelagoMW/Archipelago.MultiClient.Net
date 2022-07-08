using Archipelago.MultiClient.Net.Enums;
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

        internal PlayerHelper(ArchipelagoSocketHelper socket)
        {
            socket.PacketReceived += PacketReceived;
        }

        /// <summary>
        /// Returns the Alias corresponding to the provided player slot
        /// Alias defaults to the player's name until a different alias is specifically set
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the alias</param>
        /// <returns>The player's alias, or null if no such player is found</returns>
        public string GetPlayerAlias(int slot)
        {
            if (players == null)
            {
                return null;
            }

            PlayerInfo playerInfo = players.FirstOrDefault(p => p.Slot == slot);

            return playerInfo?.Alias;
        }

        /// <summary>
        /// Returns the Name corresponding to the provided player slot
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the name</param>
        /// <returns>The player's name, or null if no such player is found</returns>
        public string GetPlayerName(int slot)
        {
            if (players == null)
            {
                return null;
            }

            PlayerInfo playerInfo = players.FirstOrDefault(p => p.Slot == slot);

            return playerInfo?.Name;
        }

        /// <summary>
        /// Returns the Alias and Name corresponding to the provided player slot
        /// Alias defaults to the player's name until a different alias is specifically set
        /// The result is returned in the format of "Alias (Name)"
        /// </summary>
        /// <param name="slot">The slot of which to retrieve the alias</param>
        /// <returns>The player's alias and name in the following format of "Alias (Name)", or null if no such player is found</returns>
        public string GetPlayerAliasAndName(int slot)
        {
            if (players == null)
            {
                return null;
            }

            PlayerInfo playerInfo = players.FirstOrDefault(p => p.Slot == slot);
            if (playerInfo == null)
            {
                return null;
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
                case RoomUpdatePacket roomUpdatePacket:
	                OnRoomUpdatedPacketReceived(roomUpdatePacket);
	                break;
            }
        }

        private void OnConnectedPacketReceived(ConnectedPacket packet)
        {
            UpdatePlayerInfo(packet.Players);
            AddSlotInformation(packet.Team, packet.SlotInfo);
        }

        private void OnRoomUpdatedPacketReceived(RoomUpdatePacket packet)
        {
            if (packet.Players != null && packet.Players.Length > 0)
            {
                UpdatePlayerInfo(packet.Players);
            }
        }

        private void AddSlotInformation(int team, Dictionary<int, NetworkSlot> slotInfos)
        {
            if (players == null)
            {
                return;
            }

            foreach (var player in players)
            {
                if (player.Team != team)
                {
                    continue;
                }

                var slotInfoForPlayer = slotInfos[player.Slot];

                player.Game = slotInfoForPlayer.Game;
                player.SlotType = slotInfoForPlayer.Type;
                player.SlotName = slotInfoForPlayer.Name;
            }
        }

        private void UpdatePlayerInfo(NetworkPlayer[] networkPlayers)
        {
            if (players == null)
            {
                players = networkPlayers.Select(p => new PlayerInfo {
                    Team = p.Team,
                    Slot = p.Slot,
                    Name = p.Name,
                    Alias = p.Alias
                }).ToArray();
            }
            else
            {
                for (int i = 0; i < networkPlayers.Length; i++)
                {
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
        public string SlotName { get; internal set; }
        public SlotType SlotType { get; internal set; }
    }
}