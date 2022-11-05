using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Archipelago.MultiClient.Net.Cache
{
    internal interface IFileSystemDataPackageProvider
    {
        bool TryGetDataPackage(string game, out GameData gameData);
        void SaveDataPackageToFile(string game, GameData gameData);
    }

    class FileSystemDataPackageProvider : IFileSystemDataPackageProvider
    {
        private const string DataPackageFolderName = "ArchipelagoDatapackageCache";
        private readonly string cacheFolder = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DataPackageFolderName);
        private string GetFilePath(string game) => Path.Combine(cacheFolder, $"{GetFileSystemSafeFileName(game)}.json");

        private readonly object fileAccessLockObjectsLock = new object();
        private readonly Dictionary<string, object> fileAccessLockObjects = new Dictionary<string, object>();

        public bool TryGetDataPackage(string game, out GameData gameData)
        {
            var filePath = GetFilePath(game);

            lock (GetFileLock(game))
            {

                try
                {
                    if (File.Exists(filePath))
                    {
                        var fileText = File.ReadAllText(filePath);
                        gameData = JsonConvert.DeserializeObject<GameData>(fileText);
                        return gameData != null;
                    }
                }
                catch
                {
                    // ignored
                }

                gameData = null;
                return false;
            }
        }

        public void SaveDataPackageToFile(string game, GameData gameData)
        {
            Directory.CreateDirectory(cacheFolder);

            lock (GetFileLock(game))
            {
                try
                {
                    var contents = JsonConvert.SerializeObject(gameData);
                    File.WriteAllText(GetFilePath(game), contents);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private string GetFileSystemSafeFileName(string gameName)
        {
            var safeName = gameName;

            foreach (var c in Path.GetInvalidFileNameChars())
                gameName = gameName.Replace(c, '_');

            return safeName;
        }

        private object GetFileLock(string game)
        {
            lock (fileAccessLockObjectsLock)
            {
                if (fileAccessLockObjects.TryGetValue(game, out var lockObject))
                    return lockObject;

                return fileAccessLockObjects[game] = new object();
            }
        }
    }
}
