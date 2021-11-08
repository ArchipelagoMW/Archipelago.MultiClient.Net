using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.Cache
{
    internal interface IDataPackageCache
    {
        bool SaveDataPackageToCache(DataPackage package);
        bool TryGetDataPackageFromCache(out DataPackage package);
    }
}