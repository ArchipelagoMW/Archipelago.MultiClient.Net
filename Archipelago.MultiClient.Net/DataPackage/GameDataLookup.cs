using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.DataPackage
{
	interface IGameDataLookup
	{
		TwoWayLookup<long, string> Locations { get; }
		TwoWayLookup<long, string> Items { get; }
		string Checksum { get; }
	}

	class GameDataLookup : IGameDataLookup
	{
		public TwoWayLookup<long, string> Locations { get; }
		public TwoWayLookup<long, string> Items { get; }
		public string Checksum { get; }

		public GameDataLookup(GameData gameData)
		{
			Locations = new TwoWayLookup<long, string>();
			Items = new TwoWayLookup<long, string>();

			Checksum = gameData.Checksum;

			foreach (var kvp in gameData.LocationLookup)
				Locations.Add(kvp.Value, kvp.Key);

			foreach (var kvp in gameData.ItemLookup)
				Items.Add(kvp.Value, kvp.Key);
		}	
	}
}
