using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
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

        volatile bool expectingLoginResult = false;
        private LoginResult loginResult = null;

        public List<string> Tags = new List<string>();

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations,
                                    PlayerHelper players)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            Players = players;

            socket.PacketReceived += Socket_PacketReceived;
        }

        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
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
        /// <param name="itemsHandlingFlags">Informs the AP server how you want ReceivedItem packets to be sent to you.</param>
        /// <returns>
        ///     <see cref="T:Archipelago.MultiClient.Net.LoginSuccessful"/> if the connection is succeeded and the server accepted the login attempt.
        ///     <see cref="T:Archipelago.MultiClient.Net.LoginFailure"/> if the connection to the server failed in some way.
        /// </returns>
        /// <remarks>
        ///     The connect attempt is synchronous and will lock for up to 5 seconds as it attempts to connect to the server. 
        ///     Most connections are instantaneous however the timeout is 5 seconds before it returns <see cref="T:Archipelago.MultiClient.Net.LoginFailure"/>.
        /// </remarks>
        [Obsolete("Deprecated. Use the other overload for this method which requires you define the ItemHandlingFlags. This method defaults ItemHandlingFlags to null which is also deprecated.")]
        public LoginResult TryConnectAndLogin(string game, string name, Version version, List<string> tags = null, string uuid = null, string password = null, ItemsHandlingFlags? itemsHandlingFlags = null)
        {
            return TryConnectAndLogin(game, name, version, itemsHandlingFlags, tags, uuid, password);
        }

        /// <summary>
        ///     Attempt to log in to the Archipelago server by opening a websocket connection and sending a Connect packet.
        ///     Determining success for this attempt is done by attaching a listener to Socket.PacketReceived and listening for a Connected packet.
        /// </summary>
        /// <param name="game">The game this client is playing.</param>
        /// <param name="name">The slot name of this client.</param>
        /// <param name="version">The minimum AP protocol version this client supports.</param>
        /// <param name="itemsHandlingFlags">Informs the AP server how you want ReceivedItem packets to be sent to you.</param>
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
        public LoginResult TryConnectAndLogin(string game, string name, Version version, ItemsHandlingFlags? itemsHandlingFlags, List<string> tags = null, string uuid = null, string password = null)
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
                    Version = version,
                    ItemsHandling = itemsHandlingFlags
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
        [Obsolete("Deprecated. Use UpdateConnectionOptions() instead. Will be removed in next major release.")]
        public void UpdateTags(List<string> tags)
        {
            Tags = tags ?? new List<string>();

            Socket.SendPacket(new ConnectUpdatePacket
            {
                Tags = Tags
            });
        }

        /// <summary>
        ///     Send a ConnectUpdate packet and set the tags and ItemsHandlingFlags for the current connection to the provided params.
        /// </summary>
        /// <param name="tags">New tags for the current connection.</param>
        /// <param name="itemsHandlingFlags">New ItemsHandlingFlags for the current connection.</param>
        public void UpdateConnectionOptions(List<string> tags, ItemsHandlingFlags itemsHandlingFlags)
        {
            Tags = tags ?? new List<string>();

            Socket.SendPacket(new ConnectUpdatePacket
            {
                Tags = Tags,
                ItemsHandling = itemsHandlingFlags
            });
        }
    }
}