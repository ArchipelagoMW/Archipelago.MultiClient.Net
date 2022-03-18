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
        const int APConnectionTimeoutInSeconds = 5;
        public static string Game { get; internal set; }

        public ArchipelagoSocketHelper Socket { get; }
        public ReceivedItemsHelper Items { get; }
        public LocationCheckHelper Locations { get; }
        public PlayerHelper Players { get; }
        public DataStorageHelper DataStorage { get; }
        public RoomStateHelper RoomState { get; }

        volatile bool expectingLoginResult;
        private LoginResult loginResult;

        public string[] Tags = new string[0];

        public ItemsHandlingFlags ItemsHandlingFlags { get; private set; }
        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations,
                                    PlayerHelper players,
                                    RoomStateHelper roomState,
                                    DataStorageHelper dataStorage)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            Players = players;
            RoomState = roomState;
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

            uuid = uuid ?? Guid.NewGuid().ToString();
            Tags = tags ?? new string[0];
            ItemsHandlingFlags = itemsHandlingFlags;
            Game = game;

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
            Tags = tags ?? new string[0];

            Socket.SendPacket(new ConnectUpdatePacket
            {
                Tags = Tags,
                ItemsHandling = itemsHandlingFlags
            });
        }
    }
}