using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
                        Exclusions = exclusions.ToList()
                    });
                }
            }
            else if (packet.PacketType == ArchipelagoPacketType.DataPackage)
            {
                var packagePacket = (DataPackagePacket)packet;
                SaveDataPackageToCache(packagePacket.DataPackage);
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

        public bool SaveDataPackageToCache(DataPackage package)
        {
            try
            {
                if (TryGetDataPackageFromCache(out var cachedPackage))
                {
                    foreach (var item in cachedPackage.Games.Keys.ToList())
                    {
                        if (package.Games.ContainsKey(item))
                        {
                            if (cachedPackage.Games[item].Version != package.Games[item].Version)
                            {
                                if (package.Games[item].Version == 0)
                                {
                                    continue;
                                }

                                cachedPackage.Games[item] = package.Games[item];
                            }
                        }
                    }

                    foreach (var item in package.Games.Keys.ToList())
                    {
                        if (package.Games[item].Version == 0)
                        {
                            continue;
                        }

                        if (!cachedPackage.Games.ContainsKey(item))
                        {
                            cachedPackage.Games.Add(item, package.Games[item]);
                        }
                    }

                    dataPackage = package;
                    SaveDataPackageToFile(cachedPackage);
                    return true;
                }
                else
                {
                    dataPackage = package;
                    SaveDataPackageToFile(package);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void SaveDataPackageToFile(DataPackage package)
        {
            fileSystemDataPackageProvider.SaveDataPackageToFile(package);
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