using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Cache
{
    internal class DataPackageFileSystemCache : IDataPackageCache
    {
        private readonly IArchipelagoSocketHelper socket;
        private readonly IFileSystemDataPackageProvider fileSystemDataPackageProvider;

        private RoomInfoPacket roomInfoPacket;
        private DataPackage dataPackage;

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
            if (packet.PacketType == ArchipelagoPacketType.RoomInfo)
            {
                roomInfoPacket = (RoomInfoPacket)packet;
                var invalidated = GetCacheInvalidatedGames(roomInfoPacket);

                if (invalidated.Any())
                {
                    var exclusions = roomInfoPacket.DataPackageVersions.Select(x => x.Key).Except(invalidated);

                    socket.SendPacket(new GetDataPackagePacket()
                    {
                        Exclusions = exclusions.ToArray()
                    });
                }
            }
            else if (packet.PacketType == ArchipelagoPacketType.DataPackage)
            {
                var packagePacket = (DataPackagePacket)packet;
                UpdateDataPackageFromServer(packagePacket.DataPackage);
            }
        }

        public bool TryGetDataPackageFromCache(out DataPackage package)
        {
            if (dataPackage != null)
            {
                package = dataPackage;
                return true;
            }

            return fileSystemDataPackageProvider.TryGetDataPackage(out package);
        }

        internal void UpdateDataPackageFromServer(DataPackage package)
        {
            if (TryGetDataPackageFromCache(out var combinedPackage))
            {
                combinedPackage.Version = package.Version;

                foreach (var game in package.Games)
                {
                    combinedPackage.Games[game.Key] = game.Value;
                }
            }
            else
            {
                combinedPackage = package;
            }

            dataPackage = combinedPackage;
            SaveDataPackageToCache(combinedPackage);
        }
        
        private void SaveDataPackageToCache(DataPackage package)
        {
            var dataPackageForSaving = PrepareDataPackageForSaving(package);

            fileSystemDataPackageProvider.SaveDataPackageToFile(dataPackageForSaving);
        }

        private DataPackage PrepareDataPackageForSaving(DataPackage package)
        {
            var dataPackageForSaving = new DataPackage {
                Version = package.Version,
                Games = new Dictionary<string, GameData>(package.Games.Count)
            };

            foreach (var game in package.Games)
            {
                if(game.Value.Version == 0)
                    continue;

                dataPackageForSaving.Games[game.Key] = game.Value;
            }

            return dataPackageForSaving;
        }

        private List<string> GetCacheInvalidatedGames(RoomInfoPacket packet)
        {
            var gamesNeedingUpdating = new List<string>();

            if (TryGetDataPackageFromCache(out var cachedPackage))
            {
                foreach (var item in packet.DataPackageVersions)
                {
                    if (cachedPackage.Games.ContainsKey(item.Key))
                    {
                        GameData gameDataFromCache = cachedPackage.Games[item.Key];
                        if (item.Value != gameDataFromCache.Version)
                        {
                            gamesNeedingUpdating.Add(item.Key);
                        }
                    }
                    else
                    {
                        gamesNeedingUpdating.Add(item.Key);
                    }
                }
                return gamesNeedingUpdating;
            }

            return packet.DataPackageVersions.Select(x => x.Key).ToList();
        }
    }
}