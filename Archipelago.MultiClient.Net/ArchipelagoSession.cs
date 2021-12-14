using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Archipelago.MultiClient.Net
{
    public class ArchipelagoSession
    {
        const int APConnectionTimeoutInSeconds = 5;

        public ArchipelagoSocketHelper Socket { get; }

        public ReceivedItemsHelper Items { get; }

        public LocationCheckHelper Locations { get; }

        public PlayerHelper Players { get; }

        private IDataPackageCache DataPackageCache { get; }

        private bool expectingDataPackage = false;
        private Action<DataPackage> dataPackageCallback;

        volatile bool expectingLoginResult = false;
        private LoginResult loginResult = null;

        public List<string> Tags = new List<string>();

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations,
                                    PlayerHelper players,
                                    IDataPackageCache cache)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            Players = players;
            DataPackageCache = cache;

            socket.PacketReceived += Socket_PacketReceived;
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case DataPackagePacket dataPackagePacket:
                {
                    if (expectingDataPackage)
                    {
                        DataPackageCache.SaveDataPackageToCache(dataPackagePacket.DataPackage);

                        if (dataPackageCallback != null)
                        {
                            dataPackageCallback(dataPackagePacket.DataPackage);
                        }

                        expectingDataPackage = false;
                        dataPackageCallback = null;
                    }
                    break;
                }
            }

            switch (packet)
            {
                case ConnectedPacket connectedPacket:
                {
                    if (expectingLoginResult)
                    {
                        expectingLoginResult = false;
                        loginResult = new LoginSuccessful(connectedPacket);
                    }
                }
                break;
                case ConnectionRefusedPacket connectionRefusedPacket:
                {
                    if (expectingLoginResult)
                    {
                        expectingLoginResult = false;
                        loginResult = new LoginFailure(connectionRefusedPacket);
                    }
                }
                break;
            }
        }

        /// <summary>
        ///     Attempt to log in to the Archipelago server by opening a websocket connection and sending a Connect packet.
        ///     Determining success for this attempt is done by attaching a listener to Socket.PacketReceived and listening for a Connected packet.
        /// </summary>
        /// <param name="game">The game this client is playing.</param>
        /// <param name="name">The slot name of this client.</param>
        /// <param name="version">The minimum AP protocol version this client supports.</param>
        /// <param name="tags">The tags this client supports.</param>
        /// <param name="uuid">The uuid of this client.</param>
        /// <param name="password">The password to connect to this AP room.</param>
        /// <returns>
        ///     <see cref="T:Archipelago.MultiClient.Net.LoginSuccessful"/> if the connection is succeeded and the server accepted the login attempt.
        ///     <see cref="T:Archipelago.MultiClient.Net.LoginFailure"/> if the connection to the server failed in some way.
        /// </returns>
        /// <remarks>
        ///     The connect attempt is synchronous and will lock for up to 5 seconds as it attempts to connect to the server. 
        ///     Most connections are instantaneous however the timeout is 5 seconds before it returns <see cref="T:Archipelago.MultiClient.Net.LoginFailure"/>.
        /// </remarks>
        public LoginResult TryConnectAndLogin(string game, string name, Version version, List<string> tags = null, string uuid = null, string password = null)
        {
            uuid = uuid ?? Guid.NewGuid().ToString();
            Tags = tags ?? new List<string>();

            try
            {
                Socket.Connect();

                expectingLoginResult = true;
                loginResult = null;

                Socket.SendPacket(new ConnectPacket
                {
                    Game = game,
                    Name = name,
                    Password = password,
                    Tags = Tags,
                    Uuid = uuid,
                    Version = version
                });

                var connectedStartedTime = DateTime.UtcNow;
                while (expectingLoginResult)
                {
                    if (DateTime.UtcNow - connectedStartedTime > TimeSpan.FromSeconds(APConnectionTimeoutInSeconds))
                    {
                        Socket.DisconnectAsync();

                        return new LoginFailure("Connection timed out.");
                    }

                    Thread.Sleep(100);
                }

                return loginResult;
            }
            catch (ArchipelagoSocketClosedException)
            {
                return new LoginFailure("Socket closed unexpectedly.");
            }
        }

        /// <summary>
        ///     Send a ConnectUpdate packet and set the tags for the current connection to the provided <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags">
        ///     The tags with which to overwrite the current slot's tags.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive
        /// </exception>
        public void UpdateTags(List<string> tags)
        {
            Tags = tags ?? new List<string>();

            Socket.SendPacket(new ConnectUpdatePacket
            {
                Tags = Tags
            });
        }
    }

    public abstract class LoginResult
    {
        public abstract bool Successful { get; }
    }

    public class LoginSuccessful : LoginResult
    {
        public override bool Successful => true;

        public int Team { get; }
        public int Slot { get; }
        public int[] MissingChecks { get; }
        public int[] LocationsChecked { get; }
        public Dictionary<string, object> SlotData { get; }

        public LoginSuccessful(ConnectedPacket connectedPacket)
        {
            Team = connectedPacket.Team;
            Slot = connectedPacket.Slot;
            MissingChecks = connectedPacket.MissingChecks.ToArray();
            LocationsChecked = connectedPacket.LocationsChecked.ToArray();
            SlotData = connectedPacket.SlotData;
        }
    }

    public class LoginFailure : LoginResult
    {
        public override bool Successful => false;

        public ConnectionRefusedError[] ErrorCodes { get; }
        public string[] Errors { get; }

        public LoginFailure(ConnectionRefusedPacket connectionRefusedPacket)
        {
            ErrorCodes = connectionRefusedPacket.Errors.ToArray();
            Errors = ErrorCodes.Select(GetErrorMessage).ToArray();
        }

        public LoginFailure(string message)
        {
            ErrorCodes = new ConnectionRefusedError[0];
            Errors = new[] { message };
        }

        static string GetErrorMessage(ConnectionRefusedError errorCode)
        {
            switch (errorCode)
            {
                case ConnectionRefusedError.InvalidSlot:
                    return "The slot name did not match any slot on the server.";
                case ConnectionRefusedError.InvalidGame:
                    return "The slot is set to a different game on the server.";
                case ConnectionRefusedError.SlotAlreadyTaken:
                    return "The slot already has a connection with a different uuid established.";
                case ConnectionRefusedError.IncompatibleVersion:
                    return "The client and server version mismatch.";
                case ConnectionRefusedError.InvalidPassword:
                    return "The password is invalid.";
                default:
                    return $"Unknown error: {errorCode}.";
            }
        }
    }
}