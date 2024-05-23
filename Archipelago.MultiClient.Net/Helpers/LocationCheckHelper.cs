using Archipelago.MultiClient.Net.ConcurrentCollection;
using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#if NET35
using System;
#else
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Exceptions;
#endif

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Provides way to mark locations as checked
	/// </summary>
	public interface ILocationCheckHelper
    {
		/// <summary>
		/// All locations of the slot that's is connected to, both already checked and unchecked
		/// </summary>
        ReadOnlyCollection<long> AllLocations { get; }
		/// <summary>
		/// All checked locations of the slot that's is connected to
		/// </summary>
		ReadOnlyCollection<long> AllLocationsChecked { get; }
		/// <summary>
		/// All unchecked locations of the slot that's is connected to
		/// </summary>
		ReadOnlyCollection<long> AllMissingLocations { get; }

		/// <summary>
		/// event fired when new locations are checked, this can be because a location was checked remotely checked due to an !collect or locations you checked in an earlier session
		/// </summary>
        event LocationCheckHelper.CheckedLocationsUpdatedHandler CheckedLocationsUpdated;

        /// <summary>
        ///     Submit the provided location ids as checked locations.
        /// </summary>
        /// <param name="ids">
        ///     Location ids which have been checked.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        void CompleteLocationChecks(params long[] ids);

#if NET35
	    /// <summary>
	    ///     Submit the provided location ids as checked locations.
	    /// </summary>
	    /// <param name="onComplete">
	    ///     Action to be called when the async send is completed.
	    /// </param>
	    /// <param name="ids">
	    ///     Location ids which have been checked.
	    /// </param>
	    /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
	    ///     The websocket connection is not alive.
	    /// </exception>
	    void CompleteLocationChecksAsync(Action<bool> onComplete, params long[] ids);
#else
	    /// <summary>
		///     Submit the provided location ids as checked locations.
		/// </summary>
		/// <param name="ids">
		///     Location ids which have been checked.
		/// </param>
		/// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
		///     The websocket connection is not alive.
		/// </exception>
		Task CompleteLocationChecksAsync(params long[] ids);
#endif

#if NET35
	    /// <summary>
	    ///     Ask the server for the items which are present in the provided location ids.
	    /// </summary>
	    /// <param name="callback">
	    ///     An action to run once the server responds to the scout packet.
	    ///     If the argument to the action is null then the server responded with an InvalidPacket response.
	    /// </param>
	    /// <param name="createAsHint">
	    ///     If true, creates a free hint for these locations.
	    /// </param>
	    /// <param name="ids">
	    ///     The locations ids which are to be scouted.
	    /// </param>
	    /// <remarks>
	    ///     Repeated calls of this method before a LocationInfo packet is received will cause the stored
	    ///     callback to be overwritten with the most recent call. It is recommended you chain calls to this method
	    ///     within the callbacks themselves or call this only once.
	    /// </remarks>
	    /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
	    ///     The websocket connection is not alive.
	    /// </exception>
	    void ScoutLocationsAsync(Action<Dictionary<long, ScoutedItemInfo>> callback = null, bool createAsHint = false, params long[] ids);

		/// <summary>
		///     Ask the server for the items which are present in the provided location ids.
		/// </summary>
		/// <param name="callback">
		///     An action to run once the server responds to the scout packet.
		///     If the argument to the action is null then the server responded with an InvalidPacket response.
		/// </param>
		/// <param name="ids">
		///     The locations ids which are to be scouted.
		/// </param>
		/// <remarks>
		///     Repeated calls of this method before a LocationInfo packet is received will cause the stored
		///     callback to be overwritten with the most recent call. It is recommended you chain calls to this method
		///     within the callbacks themselves or call this only once.
		/// </remarks>
		/// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
		///     The websocket connection is not alive.
		/// </exception>
		void ScoutLocationsAsync(Action<Dictionary<long, ScoutedItemInfo>> callback = null, params long[] ids);
#else
		/// <summary>
		///     Ask the server for the items which are present in the provided location ids.
		/// </summary>
		/// <param name="createAsHint">
		///     If true, creates a free hint for these locations.
		/// </param>
		/// <param name="ids">
		///     The locations ids which are to be scouted.
		/// </param>
		/// <remarks>
		///     Repeated calls of this method before a LocationInfo packet is received will cause the stored
		///     callback to be overwritten with the most recent call. It is recommended you chain calls to this method
		///     within the callbacks themselves or call this only once.
		/// </remarks>
		/// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
		///     The websocket connection is not alive.
		/// </exception>
		Task<Dictionary<long, ScoutedItemInfo>> ScoutLocationsAsync(bool createAsHint, params long[] ids);

        /// <summary>
        ///     Ask the server for the items which are present in the provided location ids.
        /// </summary>
        /// <param name="ids">
        ///     The locations ids which are to be scouted.
        /// </param>
        /// <remarks>
        ///     Repeated calls of this method before a LocationInfo packet is received will cause the stored
        ///     callback to be overwritten with the most recent call. It is recommended you chain calls to this method
        ///     within the callbacks themselves or call this only once.
        /// </remarks>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        Task<Dictionary<long, ScoutedItemInfo>> ScoutLocationsAsync(params long[] ids);
#endif

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
		long GetLocationIdFromName(string game, string locationName);

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
        string GetLocationNameFromId(long locationId, string game = null);
    }

	/// <inheritdoc/>
    public class LocationCheckHelper : ILocationCheckHelper
    {
		/// <summary>
		/// delagate for the CheckedLocationsUpdated event
		/// </summary>
		/// <param name="newCheckedLocations"></param>
        public delegate void CheckedLocationsUpdatedHandler(ReadOnlyCollection<long> newCheckedLocations);
        /// <inheritdoc/>
		public event CheckedLocationsUpdatedHandler CheckedLocationsUpdated;

		readonly IConcurrentHashSet<long> allLocations = new ConcurrentHashSet<long>();
        readonly IConcurrentHashSet<long> locationsChecked = new ConcurrentHashSet<long>();
        readonly IConcurrentHashSet<long> serverConfirmedChecks = new ConcurrentHashSet<long>();
		ReadOnlyCollection<long> missingLocations = new ReadOnlyCollection<long>(new long[0]);

        readonly IArchipelagoSocketHelper socket;
        readonly IDataPackageCache cache;
        readonly IConnectionInfoProvider connectionInfoProvider;
        readonly IPlayerHelper players;

		bool awaitingLocationInfoPacket;
#if NET35
        Action<LocationInfoPacket> locationInfoPacketCallback;
#else
        TaskCompletionSource<LocationInfoPacket> locationInfoPacketCallbackTask;
#endif
	    /// <inheritdoc/>
		public ReadOnlyCollection<long> AllLocations => allLocations.AsToReadOnlyCollection();
	    /// <inheritdoc/>
		public ReadOnlyCollection<long> AllLocationsChecked => locationsChecked.AsToReadOnlyCollection();
	    /// <inheritdoc/>
		public ReadOnlyCollection<long> AllMissingLocations => missingLocations;

        internal LocationCheckHelper(IArchipelagoSocketHelper socket, IDataPackageCache cache, 
	        IConnectionInfoProvider connectionInfoProvider, IPlayerHelper players)
        {
            this.socket = socket;
            this.cache = cache;
			this.connectionInfoProvider = connectionInfoProvider;

            socket.PacketReceived += Socket_PacketReceived;
        }

        void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket connectedPacket:
                    allLocations.UnionWith(connectedPacket.LocationsChecked);
                    allLocations.UnionWith(connectedPacket.MissingChecks);
                    serverConfirmedChecks.UnionWith(connectedPacket.LocationsChecked);

					missingLocations = new ReadOnlyCollection<long>(connectedPacket.MissingChecks);

                    CheckLocations(connectedPacket.LocationsChecked);
                    break;
                case RoomUpdatePacket updatePacket:
                    CheckLocations(updatePacket.CheckedLocations);

					if (updatePacket.CheckedLocations != null)
						serverConfirmedChecks.UnionWith(updatePacket.CheckedLocations);
					break;
#if NET35
                case LocationInfoPacket locationInfoPacket:
                    if (awaitingLocationInfoPacket)
                    {
                        if (locationInfoPacketCallback != null)
                            locationInfoPacketCallback(locationInfoPacket);

                        awaitingLocationInfoPacket = false;
                        locationInfoPacketCallback = null;
                    }
                    break;
                case InvalidPacketPacket invalidPacket:
                    if (awaitingLocationInfoPacket && invalidPacket.OriginalCmd == ArchipelagoPacketType.LocationScouts)
                    {
                        locationInfoPacketCallback(null);

                        awaitingLocationInfoPacket = false;
                        locationInfoPacketCallback = null;
                    }
                    break;
#else
                case LocationInfoPacket locationInfoPacket:
                    if (awaitingLocationInfoPacket)
                    {
                        if (locationInfoPacketCallbackTask != null)
                            locationInfoPacketCallbackTask.TrySetResult(locationInfoPacket);

                        awaitingLocationInfoPacket = false;
                        locationInfoPacketCallbackTask = null;
                    }
                    break;
                case InvalidPacketPacket invalidPacket:
                    if (awaitingLocationInfoPacket && invalidPacket.OriginalCmd == ArchipelagoPacketType.LocationScouts)
                    {
                        locationInfoPacketCallbackTask.TrySetException(
                            new ArchipelagoServerRejectedPacketException(
                                invalidPacket.OriginalCmd, invalidPacket.ErrorType, 
                                $"location scout rejected by the server: {invalidPacket.ErrorText}"));

                        awaitingLocationInfoPacket = false;
                        locationInfoPacketCallbackTask = null;
                    }
                    break;
#endif
            }
        }

		/// <inheritdoc/>
		public void CompleteLocationChecks(params long[] ids)
        {
            CheckLocations(ids);

            var packet = GetLocationChecksPacket();

			if (packet.Locations.Any())
				socket.SendPacket(packet);
        }

#if NET35
	    /// <inheritdoc/>
		public void CompleteLocationChecksAsync(Action<bool> onComplete, params long[] ids)
        {
            CheckLocations(ids);

            var packet = GetLocationChecksPacket();

            if (packet.Locations.Any())
				socket.SendPacketAsync(GetLocationChecksPacket(), onComplete);
		}
#else
	    /// <inheritdoc/>
		public Task CompleteLocationChecksAsync(params long[] ids)
		{
			// ReSharper disable once ArrangeMethodOrOperatorBody
			return Task.Factory.StartNew(() =>
			{
				CheckLocations(ids);
			}).ContinueWith(t =>
			{
				var packet = GetLocationChecksPacket();

				if (packet.Locations.Any())
					socket.SendPacketAsync(GetLocationChecksPacket());
			});
		}
#endif

		LocationChecksPacket GetLocationChecksPacket() =>
		    new LocationChecksPacket
		    {
			    Locations = locationsChecked.AsToReadOnlyCollectionExcept(serverConfirmedChecks).ToArray()
		    };

#if NET35
	    /// <inheritdoc/>
		public void ScoutLocationsAsync(Action<Dictionary<long, ScoutedItemInfo>> callback = null, bool createAsHint = false, params long[] ids)
        {
            socket.SendPacketAsync(new LocationScoutsPacket()
            {
                Locations = ids,
                CreateAsHint = createAsHint
            });
            awaitingLocationInfoPacket = true;
            locationInfoPacketCallback = (scoutResult) =>
            {
				var items = scoutResult.Locations.ToDictionary(
					i => i.Location,
					i => new ScoutedItemInfo(i, connectionInfoProvider.Game, error, this,
						players.Players[connectionInfoProvider.Team][i.Player]));

				callback(items);
            };
        }

		/// <inheritdoc/>
		public void ScoutLocationsAsync(Action<Dictionary<long, ScoutedItemInfo>> callback = null, params long[] ids) =>
	        // Maintain backwards compatibility if createAsHint parameter is not specified.
	        ScoutLocationsAsync(callback, false, ids);
#else
	    /// <inheritdoc/>
		public Task<Dictionary<long, ScoutedItemInfo>> ScoutLocationsAsync(bool createAsHint, params long[] ids)
        {
            locationInfoPacketCallbackTask = new TaskCompletionSource<LocationInfoPacket>();
            awaitingLocationInfoPacket = true;

            socket.SendPacket(new LocationScoutsPacket()
            {
                Locations = ids,
                CreateAsHint = createAsHint
            });

            return locationInfoPacketCallbackTask.Task;
        }

	    /// <inheritdoc/>
        public Task<Dictionary<long, ScoutedItemInfo>> ScoutLocationsAsync(params long[] ids) =>
	        ScoutLocationsAsync(false, ids); // Maintain backwards compatibility if createAsHint parameter is not specified.
#endif

		/// <inheritdoc/>
		public long GetLocationIdFromName(string game, string locationName)
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

		/// <inheritdoc/>
		public string GetLocationNameFromId(long locationId, string game = null)
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

        void CheckLocations(ICollection<long> locationIds)
        {
            if (locationIds == null || !locationIds.Any())
                return;

            var newLocations = new List<long>();

            foreach (var locationId in locationIds)
            {
                allLocations.TryAdd(locationId);

                if (locationsChecked.TryAdd(locationId))
                    newLocations.Add(locationId);
            }

            missingLocations = allLocations.AsToReadOnlyCollectionExcept(locationsChecked);

            if (newLocations.Any())
                CheckedLocationsUpdated?.Invoke(new ReadOnlyCollection<long>(newLocations));
        }
    }
}
