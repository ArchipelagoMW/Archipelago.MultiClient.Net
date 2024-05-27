using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Archipelago.MultiClient.Net.DataPackage
{
	class FileSystemCheckSumDataPackageProvider : IFileSystemDataPackageProvider
	{
		static readonly string CacheFolder;
			
		static FileSystemCheckSumDataPackageProvider()
		{
			CacheFolder =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
					Path.Combine("Archipelago", Path.Combine("Cache", "datapackage")));
		}
		
		public bool TryGetDataPackage(string game, string checksum, out GameData gameData)
		{
			var folderPath = Path.Combine(CacheFolder, GetFileSystemSafeFileName(game));
			var filePath = Path.Combine(folderPath, $"{checksum}.json");

			if (!File.Exists(filePath))
			{
				gameData = null;
				return false;
			}

			try
			{
				var fileText = File.ReadAllText(filePath);
				gameData = JsonConvert.DeserializeObject<GameData>(fileText);
				return true;
			}
			catch
			{
				gameData = null;
				return false;
			}
		}

		public void SaveDataPackageToFile(string game, GameData gameData)
		{
			var folderPath = Path.Combine(CacheFolder, GetFileSystemSafeFileName(game));
			var filePath = Path.Combine(folderPath, $"{GetFileSystemSafeFileName(gameData.Checksum)}.json");

			try
			{
				Directory.CreateDirectory(folderPath);

				var contents = JsonConvert.SerializeObject(gameData);
				File.WriteAllText(filePath, contents);
			}
			catch
			{
				// ignored
			}
		}

		string GetFileSystemSafeFileName(string gameName)
		{
			var safeName = gameName;

			foreach (var c in Path.GetInvalidFileNameChars())
				gameName = gameName.Replace(c.ToString(), string.Empty);

			return safeName;
		}
	}
}