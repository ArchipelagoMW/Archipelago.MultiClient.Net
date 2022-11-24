using Archipelago.MultiClient.Net.Models;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.IO;

namespace Archipelago.MultiClient.Net.Cache
{
    interface IFileSystemDataPackageProvider
    {
        bool TryGetDataPackage(string game, out GameData gameData);
        void SaveDataPackageToFile(string game, GameData gameData);
    }

    class FileSystemDataPackageProvider : IFileSystemDataPackageProvider
    {
        const string DataPackageFolderName = "ArchipelagoDatapackageCache";
        readonly string cacheFolder = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DataPackageFolderName);
        string GetFilePath(string game) => Path.Combine(cacheFolder, $"{GetFileSystemSafeFileName(game)}.json");

        readonly object fileAccessLockObjectsLock = new object();
        readonly Dictionary<string, object> fileAccessLockObjects = new Dictionary<string, object>();

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

        string GetFileSystemSafeFileName(string gameName)
        {
            var safeName = gameName;

            foreach (var c in Path.GetInvalidFileNameChars())
                gameName = gameName.Replace(c, '_');

            return safeName;
        }

        object GetFileLock(string game)
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
