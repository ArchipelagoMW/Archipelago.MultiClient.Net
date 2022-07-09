using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.MultiClient.Net.Helpers
{
    public interface ILocationCheckHelper
    {
        void CompleteLocationChecks(params long[] ids);
        void CompleteLocationChecksAsync(Action<bool> onComplete, params long[] ids);
    }

    public class LocationCheckHelper : ILocationCheckHelper
    {
        public delegate void CheckedLocationsUpdatedHandler(ReadOnlyCollection<long> newCheckedLocations);
        public event CheckedLocationsUpdatedHandler CheckedLocationsUpdated;

        private readonly HashSet<long> allLocations = new HashSet<long>();
        private readonly HashSet<long> locationsChecked = new HashSet<long>();

        private readonly IArchipelagoSocketHelper socket;
        private readonly IDataPackageCache cache;

        private readonly object locationsCheckedLockObject = new object();

        private bool awaitingLocationInfoPacket;
        private Action<LocationInfoPacket> locationInfoPacketCallback;

        private Dictionary<long, string> locationIdToNameMapping = new Dictionary<long, string>();

        public ReadOnlyCollection<long> AllLocations => new ReadOnlyCollection<long>(allLocations.ToArray());
        public ReadOnlyCollection<long> AllLocationsChecked => GetCheckedLocations();
        public ReadOnlyCollection<long> AllMissingLocations => GetMissingLocations();

        private ReadOnlyCollection<long> GetCheckedLocations()
        {
            lock (locationsCheckedLockObject)
            {
                return new ReadOnlyCollection<long>(locationsChecked.ToArray());
            }
        }

        private ReadOnlyCollection<long> GetMissingLocations()
        {
            lock (locationsCheckedLockObject)
            {
                return new ReadOnlyCollection<long>(allLocations.Where(l => !locationsChecked.Contains(l)).ToArray());
            }
        }

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

                    lock (locationsCheckedLockObject)
                    {
                        CheckLocations(connectedPacket.LocationsChecked);
                    }
                    break;
                case RoomUpdatePacket updatePacket:
                    lock (locationsCheckedLockObject)
                    {
                        CheckLocations(updatePacket.CheckedLocations);
                    }
                    break;
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
            lock (locationsCheckedLockObject)
            {
                CheckLocations(ids);

                socket.SendPacket(new LocationChecksPacket()
                {
                    Locations = locationsChecked.ToArray()
                });
            }
        }

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
            lock (locationsCheckedLockObject)
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
        }

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
            if (!cache.TryGetGameDataFromCache(game, out var gameData))
            {
                return -1;
            }

            return gameData.LocationLookup.TryGetValue(locationName, out var locationId)
                ? locationId
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
            if (locationIdToNameMapping.TryGetValue(locationId, out var name))
            {
                return name;
            }

            if (!cache.TryGetDataPackageFromCache(out var dataPackage))
            {
                return null;
            }

            locationIdToNameMapping = dataPackage.Games.Select(x => x.Value).SelectMany(x => x.LocationLookup).ToDictionary(x => x.Value, x => x.Key);

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
                if (!locationsChecked.Contains(locationId))
                {
                    locationsChecked.Add(locationId);
                    newLocations.Add(locationId);
                }

                if (!allLocations.Contains(locationId))
                {
                    allLocations.Add(locationId);
                }
            }

            if (newLocations.Any())
            {
                CheckedLocationsUpdated?.Invoke(new ReadOnlyCollection<long>(newLocations));
            }
        }
    }
}
