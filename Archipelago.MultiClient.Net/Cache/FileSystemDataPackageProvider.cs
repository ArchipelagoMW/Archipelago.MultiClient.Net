using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Archipelago.MultiClient.Net.Cache
{
    internal interface IFileSystemDataPackageProvider
    {
        bool TryGetDataPackage(out DataPackage package);
        void SaveDataPackageToFile(DataPackage package);
    }

    class FileSystemDataPackageProvider : IFileSystemDataPackageProvider
    {
        private const string DataPackageFileName = "datapackagecache.archipelagocache";
        private readonly string CacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private readonly object fileAccessLockObject = new object();

        public bool TryGetDataPackage(out DataPackage package)
        {
            lock (fileAccessLockObject)
            {
                var dataPackagePath = Path.Combine(CacheFolder, DataPackageFileName);
                try
                {
                    if (File.Exists(dataPackagePath))
                    {
                        string fileText = File.ReadAllText(dataPackagePath);
                        package = JsonConvert.DeserializeObject<DataPackage>(fileText);
                        return package != null;
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

        public void SaveDataPackageToFile(DataPackage package)
        {
            lock (fileAccessLockObject)
            {
                try
                {
                    var dataPackagePath = Path.Combine(CacheFolder, DataPackageFileName);
                    string contents = JsonConvert.SerializeObject(package);
                    File.WriteAllText(dataPackagePath, contents);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
