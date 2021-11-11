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

        public ReadOnlyCollection<PlayerInfo> AllPlayers => new ReadOnlyCollection<PlayerInfo>(players);

        public PlayerHelper(ArchipelagoSocketHelper socket)
        {
            socket.PacketReceived += PacketReceived;
        }

        public string GetPlayerName(int slot)
        {
            if (players == null || slot >= players.Length || players[slot].Name == null)
            {
                return $"Slot: {slot}";
            }

            return string.IsNullOrEmpty(players[slot].Alias)
                ? players[slot].Name
                : $"{players[slot].Alias} ({players[slot].Name})";
        }


        private void PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket connectedPacket: OnConnectedPacketReceived(connectedPacket); 
                    break;
                case RoomInfoPacket roomInfoPacket: OnRoomInfoPacketReceived(roomInfoPacket);
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
                for (int i = 0; i < networkPlayers.Count; i++)
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
    }
}