using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago.MultiClient.Net.DataPackage
{
	/// <summary>
	/// Provides methods to resolve item additional information based on ideez
	/// </summary>
	public interface IItemInfoResolver
	{
		/// <summary>
		///     Perform a lookup using the DataPackage sent as a source of truth to lookup a particular item id for a particular game.
		/// </summary>
		/// <param name="itemId">
		///     Id of the item to lookup.
		/// </param>
		/// <param name="game">
		///     The game to lookup the item id for, if null will look in the game the local player is connected to.
		///		Negative item ids are always looked up under the Archipelago game
		/// </param>
		/// <returns>
		///     The name of the item as a string, or null if no such item is found.
		/// </returns>
		string GetItemName(long itemId, string game = null);

		/// <summary>
		///     Get the name of a location from its id. Useful when receiving a packet and it is necessary to find the name of the location.
		/// </summary>
		/// <param name="locationId">
		///     The Id of the location to look up the name for. Must match the contents of the datapackage.
		/// </param>
		/// <param name="game">
		///     The game to lookup the location id for, if null will look in the game the local player is connected to.
		///		Negative location ids are always looked up under the Archipelago game
		/// </param>
		/// <returns>
		///     Returns the locationName for the provided locationId, or null if no such location is found.
		/// </returns>
		string GetLocationName(long locationId, string game = null);

		/// <summary>
		///     Get the Id of a location from its name. Useful when a game knows its locations by name but not by Archipelago Id.
		/// </summary>
		/// <param name="game">
		///     The game to look up the locations from
		/// </param>
		/// <param name="locationName">
		///     The name of the location to check the Id for. Must match the contents of the datapackage.
		/// </param>
		/// <returns>
		///     Returns the locationId for the location name that was given or -1 if no location was found.
		/// </returns>
		long GetLocationId(string locationName, string game = null);
	}

	/// <inheritdoc/>
	class ItemInfoResolver : IItemInfoResolver
	{
		readonly IDataPackageCache cache;
		readonly IConnectionInfoProvider connectionInfoProvider;

		public ItemInfoResolver(IDataPackageCache cache, IConnectionInfoProvider connectionInfoProvider)
		{
			this.cache = cache;
			this.connectionInfoProvider = connectionInfoProvider;
		}

		/// <inheritdoc/>
		public string GetItemName(long itemId, string game = null)
		{
			if (game == null)
				game = connectionInfoProvider.Game ?? "Archipelago";

			if (itemId < 0)
				game = "Archipelago";

			if (!cache.TryGetGameDataFromCache(game, out var dataPackage))
				return null;

			return dataPackage.Items.TryGetValue(itemId, out var itemName)
				? itemName
				: null;
		}

		/// <inheritdoc/>
		public string GetLocationName(long locationId, string game = null)
		{
			if (game == null)
				game = connectionInfoProvider.Game ?? "Archipelago";

			if (locationId < 0)
				game = "Archipelago";

			if (!cache.TryGetGameDataFromCache(game, out var dataPackage))
				return null;

			return dataPackage.Locations.TryGetValue(locationId, out var itemName)
				? itemName
				: null;
		}

		/// <inheritdoc/>
		public long GetLocationId(string locationName, string game = null)
		{
			if (game == null)
				game = connectionInfoProvider.Game ?? "Archipelago";

			if (cache.TryGetGameDataFromCache(game, out var gameData))
				if (gameData.Locations.TryGetValue(locationName, out var locationIdInGame))
					return locationIdInGame;

			if (cache.TryGetGameDataFromCache("Archipelago", out var archipelagoData))
				if (archipelagoData.Locations.TryGetValue(locationName, out var locationIdInArchipelago))
					return locationIdInArchipelago;

			return -1; //in hindsight -1 isnt a great return here as its a valid locationid in itzelf
		}
	}
}
