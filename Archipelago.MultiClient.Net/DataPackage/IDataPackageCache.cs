using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.DataPackage
{
    interface IDataPackageCache
    {
        bool TryGetGameDataFromCache(string game, out IGameDataLookup gameData);
        bool TryGetDataPackageFromCache(out Dictionary<string, IGameDataLookup> gameData);
    }
}