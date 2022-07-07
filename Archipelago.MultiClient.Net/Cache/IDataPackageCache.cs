using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.Cache
{
    internal interface IDataPackageCache
    {
        bool TryGetDataPackageFromCache(out DataPackage package);
    }
}