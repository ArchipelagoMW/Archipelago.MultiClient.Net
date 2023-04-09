using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.Cache
{
	interface IFileSystemDataPackageProvider
	{
		bool TryGetDataPackage(string game, string checksum, out GameData gameData);
		void SaveDataPackageToFile(string game, GameData gameData);
	}
}