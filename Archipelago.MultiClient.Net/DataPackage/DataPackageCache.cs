using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.DataPackage
{
    class DataPackageCache : IDataPackageCache
    {
        readonly IArchipelagoSocketHelper socket;

        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
        readonly Dictionary<string, IGameDataLookup> inMemoryCache = new Dictionary<string, IGameDataLookup>();

		internal IFileSystemDataPackageProvider FileSystemDataPackageProvider;

		public DataPackageCache(IArchipelagoSocketHelper socket)
        {
	        this.socket = socket;

	        socket.PacketReceived += Socket_PacketReceived;
		}

		public DataPackageCache(IArchipelagoSocketHelper socket, IFileSystemDataPackageProvider fileSystemProvider)
		{
			this.socket = socket;
			
			FileSystemDataPackageProvider = fileSystemProvider;

			socket.PacketReceived += Socket_PacketReceived;
		}

		void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case RoomInfoPacket roomInfoPacket:
	                if (FileSystemDataPackageProvider == null)
						FileSystemDataPackageProvider = new FileSystemCheckSumDataPackageProvider();

					var invalidated = GetCacheInvalidatedGamesByChecksum(roomInfoPacket);
					if (invalidated.Any())
                    {
                        socket.SendPacket(new GetDataPackagePacket
                        {
                            Games = invalidated.ToArray()
                        });
                    }
                    break;
                case DataPackagePacket packagePacket:
                    UpdateDataPackageFromServer(packagePacket.DataPackage);
                    break;
            }
        }

		public bool TryGetDataPackageFromCache(out Dictionary<string, IGameDataLookup> gameData)
        {
			gameData = inMemoryCache;

            return inMemoryCache.Any();
        }

        public bool TryGetGameDataFromCache(string game, out IGameDataLookup gameData)
        {
            if (inMemoryCache.TryGetValue(game, out var cachedGameData))
            {
                gameData = cachedGameData;
                return true;
            }

            gameData = null;
            return false;
        }

        internal void UpdateDataPackageFromServer(Models.DataPackage package)
        {
            foreach (var packageGameData in package.Games)
            {
                inMemoryCache[packageGameData.Key] = new GameDataLookup(packageGameData.Value);

				FileSystemDataPackageProvider.SaveDataPackageToFile(packageGameData.Key, packageGameData.Value);
            }
        }

        List<string> GetCacheInvalidatedGamesByChecksum(RoomInfoPacket packet)
        {
			var gamesNeedingUpdating = new List<string>();

	        foreach (var game in packet.Games)
	        {
		        if (packet.DataPackageChecksums != null
				            && packet.DataPackageChecksums.TryGetValue(game, out var checksum)
				            && FileSystemDataPackageProvider.TryGetDataPackage(game, checksum, out var cachedGameData)
				            && cachedGameData.Checksum == checksum)
					inMemoryCache[game] = new GameDataLookup(cachedGameData);
				else
			        gamesNeedingUpdating.Add(game);
	        }

	        return gamesNeedingUpdating;
        }
	}
}