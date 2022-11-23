using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Cache
{
    class DataPackageFileSystemCache : IDataPackageCache
    {
        readonly IArchipelagoSocketHelper socket;
        readonly IFileSystemDataPackageProvider fileSystemDataPackageProvider;

        readonly Dictionary<string, GameData> dataPackages = new Dictionary<string, GameData>();

        public DataPackageFileSystemCache(IArchipelagoSocketHelper socket) : this(socket, new FileSystemDataPackageProvider())
        {
        }

        internal DataPackageFileSystemCache(IArchipelagoSocketHelper socket, IFileSystemDataPackageProvider fileSystemDataPackageProvider)
        {
            this.socket = socket;
            this.fileSystemDataPackageProvider = fileSystemDataPackageProvider;

            socket.PacketReceived += Socket_PacketReceived;
        }

        void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case RoomInfoPacket roomInfoPacket:
                    AddArchipelagoGame(roomInfoPacket);
                    LoadDataPackageFromFileCache(roomInfoPacket.Games);

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

        void AddArchipelagoGame(RoomInfoPacket roomInfoPacket) => 
	        roomInfoPacket.Games = roomInfoPacket.Games
		        .Concat(new[] { "Archipelago" })
		        .Distinct()
		        .ToArray();

        void LoadDataPackageFromFileCache(string[] games)
        {
            foreach (var game in games)
                if (TryGetGameDataFromFileCache(game, out var cachedPackage))
                    dataPackages[game] = cachedPackage;
        }

        bool TryGetGameDataFromFileCache(string game, out GameData gameData)
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
            foreach (var packageGameData in package.Games)
            {
                dataPackages[packageGameData.Key] = packageGameData.Value;

                if (packageGameData.Value.Version != 0)
                    fileSystemDataPackageProvider.SaveDataPackageToFile(packageGameData.Key, packageGameData.Value);
            }
        }
        
        List<string> GetCacheInvalidatedGames(RoomInfoPacket packet)
        {
            var gamesNeedingUpdating = new List<string>();

            foreach (var game in packet.Games)
            {
                if (packet.DataPackageVersions != null 
                    && packet.DataPackageVersions.ContainsKey(game) 
                    && dataPackages.TryGetValue(game, out var gameData))
                {
                    if (gameData.Version != packet.DataPackageVersions[game])
                        gamesNeedingUpdating.Add(game);
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