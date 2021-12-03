using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
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
        private const string DataPackageFileName = "datapackagecache.archipelagocache";
        private readonly ArchipelagoSocketHelper socket;
        private readonly string CacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private RoomInfoPacket roomInfoPacket;
        private DataPackage dataPackage;

        private readonly object fileAccessLockObject = new object();

        public DataPackageFileSystemCache(ArchipelagoSocketHelper socket)
        {
            this.socket = socket;

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
            lock (fileAccessLockObject)
            {
                var dataPackagePath = Path.Combine(CacheFolder, DataPackageFileName);
                try
                {
                    if (File.Exists(dataPackagePath))
                    {
                        string fileText = File.ReadAllText(dataPackagePath);
                        package = JsonConvert.DeserializeObject<DataPackage>(fileText);
                        dataPackage = package;
                        return true;
                    }
                }
                catch
                {
                    // ignored
                }

                package = null;
                return false;
            }
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
            lock (fileAccessLockObject)
            {
                var dataPackagePath = Path.Combine(CacheFolder, DataPackageFileName);
                string contents = JsonConvert.SerializeObject(package);
                File.WriteAllText(dataPackagePath, contents);
            }
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