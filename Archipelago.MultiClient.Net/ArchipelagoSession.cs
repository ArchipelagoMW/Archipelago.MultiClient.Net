using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Threading;

namespace Archipelago.MultiClient.Net
{
    public class ArchipelagoSession
    {
        private const int ArchipelagoConnectionTimeoutInSeconds = 5;

        public ArchipelagoSocketHelper Socket { get; }
        public ReceivedItemsHelper Items { get; }
        public LocationCheckHelper Locations { get; }
        public PlayerHelper Players { get; }
        public DataStorageHelper DataStorage { get; }
        public ConnectionInfoHelper ConnectionInfo { get; }
        public RoomStateHelper RoomState { get; }

        volatile bool expectingLoginResult;
        private LoginResult loginResult;

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations,
                                    PlayerHelper players,
                                    RoomStateHelper roomState,
                                    ConnectionInfoHelper connectionInfo,
                                    DataStorageHelper dataStorage)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            Players = players;
            RoomState = roomState;
            ConnectionInfo = connectionInfo;
            DataStorage = dataStorage;

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

        // ReSharper disable once UnusedMember.Global
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
        public LoginResult TryConnectAndLogin(string game, string name, Version version, ItemsHandlingFlags itemsHandlingFlags, string[] tags = null, string uuid = null, string password = null)
        {
            ConnectionInfo.SetConnectionParameters(game, tags, itemsHandlingFlags, uuid);

            try
            {
#if !NET35
                Socket.ConnectAsync().Wait(TimeSpan.FromSeconds(5));
#else
                Socket.Connect();
#endif

                expectingLoginResult = true;
                loginResult = null;

                Socket.SendPacket(new ConnectPacket
                {
                    Game = ConnectionInfo.Game,
                    Name = name,
                    Password = password,
                    Tags = ConnectionInfo.Tags,
                    Uuid = ConnectionInfo.Uuid,
                    Version = version,
                    ItemsHandling = ConnectionInfo.ItemsHandlingFlags
                });

                var connectedStartedTime = DateTime.UtcNow;
                while (expectingLoginResult)
                {
                    if (DateTime.UtcNow - connectedStartedTime > TimeSpan.FromSeconds(ArchipelagoConnectionTimeoutInSeconds))
                    {
#if !NET35
                        Socket.DisconnectAsync().Wait();
#else
                        Socket.Connect();
#endif

                        return new LoginFailure("Connection timed out.");
                    }

                    Thread.Sleep(25);
                }

                //give other handlers time to handle the ConnectedPacket so all values are available when this method returns
                Thread.Sleep(50);

                return loginResult;
            }
            catch (ArchipelagoSocketClosedException)
            {
                return new LoginFailure("Socket closed unexpectedly.");
            }
        }

        /// <summary>
        ///     Send a ConnectUpdate packet and set the tags and ItemsHandlingFlags for the current connection to the provided params.
        /// </summary>
        /// <param name="tags">New tags for the current connection.</param>
        /// <param name="itemsHandlingFlags">New ItemsHandlingFlags for the current connection.</param>
        public void UpdateConnectionOptions(string[] tags, ItemsHandlingFlags itemsHandlingFlags)
        {
            ConnectionInfo.SetConnectionParameters(ConnectionInfo.Game, tags, itemsHandlingFlags, ConnectionInfo.Uuid);

            Socket.SendPacket(new ConnectUpdatePacket
            {
                Tags = ConnectionInfo.Tags,
                ItemsHandling = ConnectionInfo.ItemsHandlingFlags
            });
        }
    }
}