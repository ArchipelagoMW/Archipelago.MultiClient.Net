using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Threading;

#if !NET35
using System.Threading.Tasks;
#endif

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
        public MessageLogHelper MessageLog { get; }

#if NET35
        volatile bool expectingLoginResult;
        private LoginResult loginResult;
#else
        private TaskCompletionSource<LoginResult> loginResultTask = new TaskCompletionSource<LoginResult>();
        private TaskCompletionSource<RoomInfoPacket> roomInfoPacketTask = new TaskCompletionSource<RoomInfoPacket>();
#endif

        internal ArchipelagoSession(ArchipelagoSocketHelper socket,
                                    ReceivedItemsHelper items,
                                    LocationCheckHelper locations,
                                    PlayerHelper players,
                                    RoomStateHelper roomState,
                                    ConnectionInfoHelper connectionInfo,
                                    DataStorageHelper dataStorage,
                                    MessageLogHelper messageLog)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            Players = players;
            RoomState = roomState;
            ConnectionInfo = connectionInfo;
            DataStorage = dataStorage;
            MessageLog = messageLog;
            
            socket.PacketReceived += Socket_PacketReceived;
        }


        private void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {

                case ConnectedPacket _:
                case ConnectionRefusedPacket _:
#if NET35
                    if (expectingLoginResult)
                    {
                        expectingLoginResult = false;
                        loginResult = LoginResult.FromPacket(packet);
                    }
                    break;
#else
                    loginResultTask.TrySetResult(LoginResult.FromPacket(packet));
                    break;
                case RoomInfoPacket roomInfoPacket:
                    roomInfoPacketTask.TrySetResult(roomInfoPacket);
                    break;
#endif
            }
        }

#if !NET35
        /// <summary>
        /// Connect the websocket to the server
        /// Will wait a few seconds to retrieve the RoomInfoPacket, if the request timesout the task will be canceled
        /// </summary>
        /// <returns>The roominfo send from the server</returns>
        public Task<RoomInfoPacket> ConnectAsync()
        {
            roomInfoPacketTask = new TaskCompletionSource<RoomInfoPacket>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var task = Socket.ConnectAsync();
                    task.Wait(TimeSpan.FromSeconds(ArchipelagoConnectionTimeoutInSeconds));

                    if (!task.IsCompleted)
                        roomInfoPacketTask.TrySetCanceled();
                }
                catch (AggregateException)
                {
                    roomInfoPacketTask.TrySetCanceled();
                }
            });

            return roomInfoPacketTask.Task;
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
        public Task<LoginResult> LoginAsync(string game, string name, ItemsHandlingFlags itemsHandlingFlags,
            Version version = null, string[] tags = null, string uuid = null, string password = null)
        {
            if (!roomInfoPacketTask.Task.IsCompleted)
            {
                loginResultTask = new TaskCompletionSource<LoginResult>();
                loginResultTask.SetResult(new LoginFailure("You are not connected, run ConnectAsync() first"));
                return loginResultTask.Task;
            }

            ConnectionInfo.SetConnectionParameters(game, tags, itemsHandlingFlags, uuid);

            try
            {
                Socket.SendPacket(new ConnectPacket {
                    Game = ConnectionInfo.Game,
                    Name = name,
                    Password = password,
                    Tags = ConnectionInfo.Tags,
                    Uuid = ConnectionInfo.Uuid,
                    Version = version != null ? new NetworkVersion(version) : new NetworkVersion(0,3,3),
                    ItemsHandling = ConnectionInfo.ItemsHandlingFlags
                });
            }
            catch (ArchipelagoSocketClosedException)
            {
                loginResultTask = new TaskCompletionSource<LoginResult>();
                loginResultTask.SetResult(new LoginFailure("You are not connected, run ConnectAsync() first"));
                return loginResultTask.Task;
            }

            loginResultTask = SetResultAfterTimeout<LoginResult>(ArchipelagoConnectionTimeoutInSeconds, new LoginFailure("Connection timed out."));
            return loginResultTask.Task;
        }

        private static TaskCompletionSource<T> SetResultAfterTimeout<T>(int timeoutInSeconds, T result)
        {
            var task = new TaskCompletionSource<T>();
#if NET40
            var timer = new Timer(_ => task.TrySetResult(result));
            timer.Change(TimeSpan.FromSeconds(timeoutInSeconds), TimeSpan.FromMilliseconds(-1));
#else
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
            tokenSource.Token.Register(() => task.TrySetResult(result));
#endif
            return task;
        }
#endif

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
        public LoginResult TryConnectAndLogin(string game, string name, ItemsHandlingFlags itemsHandlingFlags, Version version = null, string[] tags = null, string uuid = null, string password = null)
        {
#if NET35
            ConnectionInfo.SetConnectionParameters(game, tags, itemsHandlingFlags, uuid);

            try
            {
                expectingLoginResult = true;
                loginResult = null;

                Socket.Connect();

                Socket.SendPacket(new ConnectPacket
                {
                    Game = ConnectionInfo.Game,
                    Name = name,
                    Password = password,
                    Tags = ConnectionInfo.Tags,
                    Uuid = ConnectionInfo.Uuid,
                    Version = version != null ? new NetworkVersion(version) : new NetworkVersion(0,3,3),
                    ItemsHandling = ConnectionInfo.ItemsHandlingFlags
                });

                var connectedStartedTime = DateTime.UtcNow;
                while (expectingLoginResult)
                {
                    if (DateTime.UtcNow - connectedStartedTime > TimeSpan.FromSeconds(ArchipelagoConnectionTimeoutInSeconds))
                    {
                        Socket.Disconnect();

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
#else
            var task = ConnectAsync();

            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                if (e.GetBaseException() is OperationCanceledException)
                    return new LoginFailure("Connection timed out.");

                return new LoginFailure(e.GetBaseException().Message);
            }

            return LoginAsync(game, name, itemsHandlingFlags, version, tags, uuid, password).Result;
#endif
        }
    }
}