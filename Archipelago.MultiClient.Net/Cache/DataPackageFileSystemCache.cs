using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Cache
{
    /// <summary>
    /// I already don't like this class. I think it needs re-working when I'm not coding tired.
    /// </summary>
    internal class DataPackageFileSystemCache : IDataPackageCache
    {
        private const string DataPackageFileName = "datapackagecache.archipelago";
        private readonly ArchipelagoSocketHelper socket;
        private string CacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private RoomInfoPacket roomInfoPacket;

        public DataPackageFileSystemCache(ArchipelagoSocketHelper socket)
        {
            this.socket = socket;

            socket.PacketReceived += Socket_PacketReceived;
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            // TODO: don't download datapackage for games that aren't played at all in the session
            if (packet.PacketType == ArchipelagoPacketType.RoomInfo)
            {
                roomInfoPacket = (RoomInfoPacket)packet;
                var invalidated = GetCacheInvalidatedGames(roomInfoPacket);
                var exclusions = roomInfoPacket.DataPackageVersions.Select(x => x.Key).Except(invalidated);

                socket.SendPacket(new GetDataPackagePacket()
                {
                    Exclusions = exclusions.ToList()
                });
            }
            else if (packet.PacketType == ArchipelagoPacketType.DataPackage)
            {
                var packagePacket = (DataPackagePacket)packet;
                SaveDataPackageToCache(packagePacket.DataPackage);
            }
        }

        public bool TryGetDataPackageFromCache(out DataPackage package)
        {
            var dataPackagePath = Path.Combine(CacheFolder, DataPackageFileName);
            if (File.Exists(dataPackagePath))
            {
                try
                {
                    string fileText = File.ReadAllText(dataPackagePath);
                    package = JsonConvert.DeserializeObject<DataPackage>(fileText);
                    return true;
                }
                catch (Exception e)
                {
                    throw new CacheLoadFailureException("Could not load data package cache from file system.", e);
                }
            }
            else
            {
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
                                cachedPackage.Games[item] = package.Games[item];
                            }
                        }
                    }
                    SaveDataPackageToFile(cachedPackage);
                    return true;
                }
                else
                {
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
            var dataPackagePath = Path.Combine(CacheFolder, DataPackageFileName);
            string contents = JsonConvert.SerializeObject(package);
            File.WriteAllText(dataPackagePath, contents);
        }

        private List<string> GetCacheInvalidatedGames(RoomInfoPacket packet)
        {
            var gamesNeedingUpdating = new List<string>();
            try
            {
                if (TryGetDataPackageFromCache(out var cachedPackage))
                {
                    foreach (var item in packet.DataPackageVersions)
                    {
                        GameData gameDataFromCache = cachedPackage.Games[item.Key];
                        if (item.Value != gameDataFromCache.Version)
                        {
                            gamesNeedingUpdating.Add(item.Key);
                        }
                    }
                    return gamesNeedingUpdating;
                }

                return packet.DataPackageVersions.Select(x => x.Key).ToList();
            }
            catch (CacheLoadFailureException)
            {
                return packet.DataPackageVersions.Select(x => x.Key).ToList();
            }
        }
    }
}