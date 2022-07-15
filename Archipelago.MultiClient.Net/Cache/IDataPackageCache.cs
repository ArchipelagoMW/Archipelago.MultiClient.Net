using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.Cache
{
    internal interface IDataPackageCache
    {
        bool TryGetGameDataFromCache(string game, out GameData package);
        bool TryGetDataPackageFromCache(out DataPackage package);
    }
}