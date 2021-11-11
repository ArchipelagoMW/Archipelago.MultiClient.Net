using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class LocationCheckHelper
    {
        private readonly List<int> locationsChecked = new List<int>();
        private readonly ArchipelagoSocketHelper socket;
        private readonly IDataPackageCache cache;
        private readonly object locationsCheckedLockObject = new object();
        private DataPackage dataPackage;
        private bool awaitingLocationInfoPacket;
        private Action<LocationInfoPacket> locationInfoPacketCallback;
        private Dictionary<string, Dictionary<string, int>> gameLocationNameToIdMapping;
        private Dictionary<int, string> locationIdToNameMapping;

        public ReadOnlyCollection<int> AllLocationsChecked => GetCheckedLocations();

        ReadOnlyCollection<int> GetCheckedLocations()
        {
            lock (locationsCheckedLockObject)
            {
                return new ReadOnlyCollection<int>(locationsChecked);
            }
        }

        internal LocationCheckHelper(ArchipelagoSocketHelper socket, IDataPackageCache cache)
        {
            this.socket = socket;
            this.cache = cache;
            socket.PacketReceived += Socket_PacketReceived;
        }

        /// <summary>
        ///     Submit the provided location ids as checked locations.
        /// </summary>
        /// <param name="ids">
        ///     Location ids which have been checked.
        /// </param>
        public void CompleteLocationChecks(params int[] ids)
        {
            lock (locationsCheckedLockObject)
            {
                locationsChecked.AddRange(ids);
                socket.SendPacket(new LocationChecksPacket()
                {
                    Locations = locationsChecked
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
        public void CompleteLocationChecksAsync(Action<bool> onComplete, params int[] ids)
        {
            lock (locationsCheckedLockObject)
            {
                locationsChecked.AddRange(ids);
                socket.SendPacketAsync(
                    new LocationChecksPacket()
                    {
                        Locations = locationsChecked
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
        /// <param name="ids">
        ///     The locations ids which are to be scouted.
        /// </param>
        /// <remarks>
        ///     Repeated calls of this method before a LocationInfo packet is received will cause the stored
        ///     callback to be overwritten with the most recent call. It is recommended you chain calls to this method
        ///     within the callbacks themselves or call this only once.
        /// </remarks>
        public void ScoutLocationsAsync(Action<LocationInfoPacket> callback = null, params int[] ids)
        {
            socket.SendPacketAsync(new LocationScoutsPacket()
            {
                Locations = ids.ToList()
            });
            awaitingLocationInfoPacket = true;
            locationInfoPacketCallback = callback;
        }

        /// <summary>
        ///     Get the Id of a location from its name. Useful when a game knows its locations by name but not by Archipelago Id.
        /// </summary>
        /// <param name="locationName">
        ///     The name of the location to check the Id for. Must match the contents of the datapackage.
        /// </param>
        public int GetLocationIdFromName(string game, string locationName)
        {
            if (dataPackage == null)
            {
                cache.TryGetDataPackageFromCache(out dataPackage);
            }

            if (gameLocationNameToIdMapping == null)
            {
                gameLocationNameToIdMapping = dataPackage.Games.ToDictionary(x => x.Key, x => x.Value.LocationLookup.ToDictionary(y => y.Key, y => y.Value));
            }

            return gameLocationNameToIdMapping[game][locationName];
        }

        /// <summary>
        ///     Get the name of a location from its id. Useful when receiving a packet and it is necessary to find the name of the location.
        /// </summary>
        /// <param name="locationId">
        ///     The Id of the location to look up the name for. Must match the contents of the datapackage.
        /// </param>
        public string GetLocationNameFromId(int locationId)
        {
            if (dataPackage == null)
            {
                cache.TryGetDataPackageFromCache(out dataPackage);
            }

            if (locationIdToNameMapping == null)
            {
                locationIdToNameMapping = dataPackage.Games.Select(x => x.Value).SelectMany(x => x.LocationLookup).ToDictionary(x => x.Value, x => x.Key);
            }

            return locationIdToNameMapping[locationId];
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet.PacketType)
            {
                case ArchipelagoPacketType.LocationInfo:
                {
                    if (awaitingLocationInfoPacket)
                    {
                        var infoPacket = (LocationInfoPacket)packet;

                        if (locationInfoPacketCallback != null)
                        {
                            locationInfoPacketCallback(infoPacket);
                        }

                        awaitingLocationInfoPacket = false;
                        locationInfoPacketCallback = null;
                    }
                    break;
                }
                case ArchipelagoPacketType.InvalidPacket:
                {
                    if (awaitingLocationInfoPacket)
                    {
                        var invalidPacket = (InvalidPacketPacket)packet;
                        if (invalidPacket.OriginalCmd == ArchipelagoPacketType.LocationScouts)
                        {
                            locationInfoPacketCallback(null);

                            awaitingLocationInfoPacket = false;
                            locationInfoPacketCallback = null;
                        }
                    }
                    break;
                }
            }
        }
    }
}
