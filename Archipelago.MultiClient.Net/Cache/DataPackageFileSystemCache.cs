using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Cache
{
    internal class DataPackageFileSystemCache : IDataPackageCache
    {
        private readonly IArchipelagoSocketHelper socket;
        private readonly IFileSystemDataPackageProvider fileSystemDataPackageProvider;

        private readonly Dictionary<string, GameData> dataPackages = new Dictionary<string, GameData>();

        public DataPackageFileSystemCache(IArchipelagoSocketHelper socket) : this(socket, new FileSystemDataPackageProvider())
        {
        }

        internal DataPackageFileSystemCache(IArchipelagoSocketHelper socket, IFileSystemDataPackageProvider fileSystemDataPackageProvider)
        {
            this.socket = socket;
            this.fileSystemDataPackageProvider = fileSystemDataPackageProvider;

            socket.PacketReceived += Socket_PacketReceived;
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case RoomInfoPacket roomInfoPacket:
                    AddArchipelagoGame(roomInfoPacket);
                    LoadDataPackageFromFileCache(roomInfoPacket);

                    var invalidated = GetCacheInvalidatedGames(roomInfoPacket);

                    if (invalidated.Any())
                    {
                        socket.SendPacket(new GetDataPackagePacket
                        {
                            Games = invalidated.ToArray()
                        });
                    }
                    break;
                case DataPackagePacket packagePacket:
                    UpdateDataPackageFromServer(packagePacket.DataPackage);
                    break;
            }
        }

        private void AddArchipelagoGame(RoomInfoPacket roomInfoPacket)
        {
            roomInfoPacket.Games = roomInfoPacket.Games.Concat(new[] { "Archipelago" }).ToArray();
        }

        private void LoadDataPackageFromFileCache(RoomInfoPacket packet)
        {
            foreach (var game in packet.Games.Distinct())
            {
                if (TryGetGameDataFromFileCache(game, out var cachedPackage))
                {
                    dataPackages[game] = cachedPackage;
                }
            }
        }

        private bool TryGetGameDataFromFileCache(string game, out GameData gameData)
        {
            if (fileSystemDataPackageProvider.TryGetDataPackage(game, out var cachedGameData))
            {
                gameData = cachedGameData;
                return true;
            }

            gameData = null;
            return false;
        }

        public bool TryGetDataPackageFromCache(out DataPackage package)
        {
            package = new DataPackage { Games = dataPackages };

            return dataPackages.Count > 1;
        }

        public bool TryGetGameDataFromCache(string game, out GameData gameData)
        {
            if (dataPackages.TryGetValue(game, out var cachedGameData))
            {
                gameData = cachedGameData;
                return true;
            }

            gameData = null;
            return false;
        }

        internal void UpdateDataPackageFromServer(DataPackage package)
        {
            foreach (KeyValuePair<string, GameData> packageGameData in package.Games)
            {
                dataPackages[packageGameData.Key] = packageGameData.Value;

                if (packageGameData.Value.Version != 0)
                    fileSystemDataPackageProvider.SaveDataPackageToFile(packageGameData.Key, packageGameData.Value);
            }
        }
        
        private List<string> GetCacheInvalidatedGames(RoomInfoPacket packet)
        {
            var gamesNeedingUpdating = new List<string>();

            foreach (var game in packet.Games.Distinct())
            {
                if (dataPackages.TryGetValue(game, out var gameData))
                {
                    if (gameData.Version != packet.DataPackageVersions[game])
                    {
                        gamesNeedingUpdating.Add(game);
                    }
                }
                else
                {
                    gamesNeedingUpdating.Add(game);
                }
            }

            return gamesNeedingUpdating;
        }
    }
}