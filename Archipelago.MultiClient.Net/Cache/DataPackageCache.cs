using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Cache
{
    class DataPackageCache : IDataPackageCache
    {
        readonly IArchipelagoSocketHelper socket;

        readonly Dictionary<string, GameData> dataPackages = new Dictionary<string, GameData>();

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

		static void AddArchipelagoGame(RoomInfoPacket roomInfoPacket) => 
	        roomInfoPacket.Games = roomInfoPacket.Games
		        .Concat(new[] { "Archipelago" })
		        .Distinct()
		        .ToArray();

		public bool TryGetDataPackageFromCache(out DataPackage package)
        {
            package = new DataPackage { Games = dataPackages };

            return dataPackages.Count > 1;
        }

        public bool TryGetGameDataFromCache(string game, out GameData gameData)
        {
            if (dataPackages.TryGetValue(game, out var cachedGameData))
            {
                gameData = cachedGameData;
                return true;
            }

            gameData = null;
            return false;
        }

        internal void UpdateDataPackageFromServer(DataPackage package)
        {
            foreach (var packageGameData in package.Games)
            {
                dataPackages[packageGameData.Key] = packageGameData.Value;

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
				            && FileSystemDataPackageProvider.TryGetDataPackage(game, checksum, out var gameData)
				            && gameData.Checksum == checksum)
					dataPackages[game] = gameData;
		        else
			        gamesNeedingUpdating.Add(game);
	        }

	        return gamesNeedingUpdating;
        }
	}
}