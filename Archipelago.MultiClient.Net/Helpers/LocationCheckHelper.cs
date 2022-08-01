using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.ConcurrentCollection;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#if !NET35
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Exceptions;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
    public interface ILocationCheckHelper
    {
        void CompleteLocationChecks(params long[] ids);

#if NET35
        void CompleteLocationChecksAsync(Action<bool> onComplete, params long[] ids);
#else
        Task CompleteLocationChecksAsync(params long[] ids);
#endif
    }

    public class LocationCheckHelper : ILocationCheckHelper
    {
        public delegate void CheckedLocationsUpdatedHandler(ReadOnlyCollection<long> newCheckedLocations);
        public event CheckedLocationsUpdatedHandler CheckedLocationsUpdated;

        private readonly IConcurrentHashSet<long> allLocations = new ConcurrentHashSet<long>();
        private readonly IConcurrentHashSet<long> locationsChecked = new ConcurrentHashSet<long>();
        private ReadOnlyCollection<long> missingLocations = new ReadOnlyCollection<long>(new long[0]);

        private readonly IArchipelagoSocketHelper socket;
        private readonly IDataPackageCache cache;

        private bool awaitingLocationInfoPacket;
#if NET35
        private Action<LocationInfoPacket> locationInfoPacketCallback;
#else
        private TaskCompletionSource<LocationInfoPacket> locationInfoPacketCallbackTask;
#endif

        private Dictionary<string, Dictionary<string, long>> gameLocationNameToIdMapping;
        private Dictionary<long, string> locationIdToNameMapping;

        public ReadOnlyCollection<long> AllLocations => allLocations.AsToReadOnlyCollection();
        public ReadOnlyCollection<long> AllLocationsChecked => locationsChecked.AsToReadOnlyCollection();
        public ReadOnlyCollection<long> AllMissingLocations => missingLocations;

        internal LocationCheckHelper(IArchipelagoSocketHelper socket, IDataPackageCache cache)
        {
            this.socket = socket;
            this.cache = cache;

            socket.PacketReceived += Socket_PacketReceived;
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket connectedPacket:
                    allLocations.UnionWith(connectedPacket.LocationsChecked);
                    allLocations.UnionWith(connectedPacket.MissingChecks);

                    missingLocations = new ReadOnlyCollection<long>(connectedPacket.MissingChecks);

                     CheckLocations(connectedPacket.LocationsChecked);
                    break;
                case RoomUpdatePacket updatePacket:
                    CheckLocations(updatePacket.CheckedLocations);
                    break;
#if NET35
                case LocationInfoPacket locationInfoPacket:
                    if (awaitingLocationInfoPacket)
                    {
                        if (locationInfoPacketCallback != null)
                        {
                            locationInfoPacketCallback(locationInfoPacket);
                        }

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
                        {
                            locationInfoPacketCallbackTask.SetResult(locationInfoPacket);
                        }

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

        /// <summary>
        ///     Submit the provided location ids as checked locations.
        /// </summary>
        /// <param name="ids">
        ///     Location ids which have been checked.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        public void CompleteLocationChecks(params long[] ids)
        {
            CheckLocations(ids);

            socket.SendPacket(new LocationChecksPacket()
            {
                Locations = locationsChecked.ToArray()
            });
        }

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
        public void CompleteLocationChecksAsync(Action<bool> onComplete, params long[] ids)
        {
            CheckLocations(ids);

            socket.SendPacketAsync(
                new LocationChecksPacket()
                {
                    Locations = locationsChecked.ToArray()
                },
                onComplete
            );
        }
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
        public Task CompleteLocationChecksAsync(params long[] ids)
        {
            CheckLocations(ids);

            return socket.SendPacketAsync(
                new LocationChecksPacket()
                {
                    Locations = locationsChecked.ToArray()
                });
        }

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
        public void ScoutLocationsAsync(Action<LocationInfoPacket> callback = null, bool createAsHint = false, params long[] ids)
        {
            socket.SendPacketAsync(new LocationScoutsPacket()
            {
                Locations = ids,
                CreateAsHint = createAsHint
            });
            awaitingLocationInfoPacket = true;
            locationInfoPacketCallback = callback;
        }

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
        public void ScoutLocationsAsync(Action<LocationInfoPacket> callback = null, params long[] ids)
        {
            // Maintain backwards compatibility if createAsHint parameter is not specified.
            ScoutLocationsAsync(callback, false, ids);
        }
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
        public Task<LocationInfoPacket> ScoutLocationsAsync(bool createAsHint, params long[] ids)
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
        public Task<LocationInfoPacket> ScoutLocationsAsync(params long[] ids)
        {
            // Maintain backwards compatibility if createAsHint parameter is not specified.
            return ScoutLocationsAsync(false, ids);
        }
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
        public long GetLocationIdFromName(string game, string locationName)
        {
            if (!cache.TryGetDataPackageFromCache(out var dataPackage))
            {
                return -1;
            }

            if (gameLocationNameToIdMapping == null)
            {
                gameLocationNameToIdMapping = dataPackage.Games.ToDictionary(x => x.Key, x => x.Value.LocationLookup.ToDictionary(y => y.Key, y => y.Value));
            }

            return gameLocationNameToIdMapping.TryGetValue(game, out var locationNameToIdLookup)
                ? locationNameToIdLookup.TryGetValue(locationName, out var locationId)
                    ? locationId
                    : -1
                : -1;
        }

        /// <summary>
        ///     Get the name of a location from its id. Useful when receiving a packet and it is necessary to find the name of the location.
        /// </summary>
        /// <param name="locationId">
        ///     The Id of the location to look up the name for. Must match the contents of the datapackage.
        /// </param>
        /// <returns>
        ///     Returns the locationName for the provided locationId, or null if no such location is found.
        /// </returns>
        public string GetLocationNameFromId(long locationId)
        {
            if (!cache.TryGetDataPackageFromCache(out var dataPackage))
            {
                return null;
            }

            if (locationIdToNameMapping == null)
            {
                locationIdToNameMapping = dataPackage.Games.Select(x => x.Value).SelectMany(x => x.LocationLookup).ToDictionary(x => x.Value, x => x.Key);
            }

            return locationIdToNameMapping.TryGetValue(locationId, out var locationName)
                ? locationName
                : null;
        }

        private void CheckLocations(ICollection<long> locationIds)
        {
            if (locationIds == null || !locationIds.Any())
                return;

            List<long> newLocations = new List<long>();

            foreach (long locationId in locationIds)
            {
                allLocations.TryAdd(locationId);

                if (locationsChecked.TryAdd(locationId))
                {
                    newLocations.Add(locationId);
                }
            }

            missingLocations = allLocations.AsToReadOnlyCollectionExcept(locationsChecked);

            if (newLocations.Any())
            {
                CheckedLocationsUpdated?.Invoke(new ReadOnlyCollection<long>(newLocations));
            }
        }
    }
}
