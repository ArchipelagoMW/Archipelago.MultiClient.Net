using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

#if !NET35
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net
{
	/// <summary>
	/// `ArchipelagoSession` is the overarching class to access and modify and respond to the multiworld state
	/// </summary>
	public class ArchipelagoSession
    {
        const int ArchipelagoConnectionTimeoutInSeconds = 4;

		/// <summary>
		/// Provides access to the underlying websocket, so send or receive messages
		/// </summary>
		public IArchipelagoSocketHelper Socket { get; }

		/// <summary>
		/// Provides simplified methods to receive items
		/// </summary>
        public IReceivedItemsHelper Items { get; }

		/// <summary>
		/// Provides way to mark locations as checked
		/// </summary>
        public ILocationCheckHelper Locations { get; }

		/// <summary>
		/// Provides information about all players in the multiworld
		/// </summary>
        public IPlayerHelper Players { get; }

		/// <summary>
		/// Provides access to a server side data storage to share and store values across sessions and players
		/// </summary>
        public IDataStorageHelper DataStorage { get; }

		/// <summary>
		/// Provides information about your current connection
		/// </summary>
        public IConnectionInfoProvider ConnectionInfo => connectionInfo;
		ConnectionInfoHelper connectionInfo;

		/// <summary>
		/// Provides information about the current state of the server
		/// </summary>
		public IRoomStateHelper RoomState { get; }

		/// <summary>
		/// Provides simplified handlers to receive messages about events that happen in the multiworld
		/// </summary>
        public IMessageLogHelper MessageLog { get; }

#if NET35
	    volatile bool awaitingRoomInfo;
		volatile bool expectingLoginResult;
        LoginResult loginResult;
#else
        TaskCompletionSource<LoginResult> loginResultTask = new TaskCompletionSource<LoginResult>();
        TaskCompletionSource<RoomInfoPacket> roomInfoPacketTask = new TaskCompletionSource<RoomInfoPacket>();
#endif

		internal ArchipelagoSession(IArchipelagoSocketHelper socket,
                                    IReceivedItemsHelper items,
                                    ILocationCheckHelper locations,
                                    IPlayerHelper players,
                                    IRoomStateHelper roomState,
                                    ConnectionInfoHelper connectionInfoHelper,
                                    IDataStorageHelper dataStorage,
                                    IMessageLogHelper messageLog)
        {
            Socket = socket;
            Items = items;
            Locations = locations;
            Players = players;
            RoomState = roomState;
            connectionInfo = connectionInfoHelper;
            DataStorage = dataStorage;
            MessageLog = messageLog;
            
            socket.PacketReceived += Socket_PacketReceived;
        }
		
        void Socket_PacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket _:
	            case ConnectionRefusedPacket _:
					if (packet is ConnectedPacket && RoomState.Version != null && RoomState.Version >= new Version(0, 3, 8))
						LogUsedVersion();

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
#endif
                // ReSharper disable once UnusedVariable
                case RoomInfoPacket roomInfoPacket:
#if NET35
					awaitingRoomInfo = false;
#else
					roomInfoPacketTask.TrySetResult(roomInfoPacket);
#endif
					break;
            }
        }

        void LogUsedVersion()
        {
#if NET35
			const string libVersion = "NET35";
#elif NET40
			const string libVersion = "NET40";
#elif NET45
			const string libVersion = "NET45";
#elif NETSTANDARD2_0
			const string libVersion = "NETSTANDARD2_0";
#elif NET6_0
			const string libVersion = "NET6_0";
#else
			const string libVersion = "OTHER";
#endif
            try
            {
				var assemblyVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
				
		        Socket.SendPacketAsync(new SetPacket {
			        Key = ".NetUsedVersions",
			        DefaultValue = JObject.FromObject(new Dictionary<string, bool>()),
			        Operations = new[] {
						Operation.Update(new Dictionary<string, bool> {
							{ $"{ConnectionInfo.Game}:{assemblyVersion}:{libVersion}", true }
						})
			        }
		        });
	        }
	        catch
	        {
		        // ignored
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
		/// <param name="requestSlotData">Decides if the <see cref="LoginSuccessful"/> result will contain any slot data</param>
		/// <returns>
		///     <see cref="T:Archipelago.MultiClient.Net.LoginSuccessful"/> if the connection is succeeded and the server accepted the login attempt.
		///     <see cref="T:Archipelago.MultiClient.Net.LoginFailure"/> if the connection to the server failed in some way.
		/// </returns>
		/// <remarks>
		///     The connect attempt is synchronous and will lock for up to 5 seconds as it attempts to connect to the server. 
		///     Most connections are instantaneous however the timeout is 5 seconds before it returns <see cref="T:Archipelago.MultiClient.Net.LoginFailure"/>.
		/// </remarks>
		public Task<LoginResult> LoginAsync(string game, string name, ItemsHandlingFlags itemsHandlingFlags,
            Version version = null, string[] tags = null, string uuid = null, string password = null, bool requestSlotData = true)
        {
	        loginResultTask = new TaskCompletionSource<LoginResult>();

			if (!roomInfoPacketTask.Task.IsCompleted)
            {
                loginResultTask = new TaskCompletionSource<LoginResult>();
                loginResultTask.TrySetResult(new LoginFailure("You are not connected, run ConnectAsync() first"));
                return loginResultTask.Task;
            }

			connectionInfo.SetConnectionParameters(game, tags, itemsHandlingFlags, uuid);

            try
            {
	            Socket.SendPacket(BuildConnectPacket(name, password, version, requestSlotData));
            }
            catch (ArchipelagoSocketClosedException)
            {
                loginResultTask.TrySetResult(new LoginFailure("You are not connected, run ConnectAsync() first"));
                return loginResultTask.Task;
            }

            SetResultAfterTimeout<LoginResult>(loginResultTask, ArchipelagoConnectionTimeoutInSeconds, 
	            new LoginFailure("Connection timed out."));

            return loginResultTask.Task;
        }

        static void SetResultAfterTimeout<T>(TaskCompletionSource<T> task, int timeoutInSeconds, T result)
        {
#if NET40
            var timer = new Timer(_ => task.TrySetResult(result));
            timer.Change(TimeSpan.FromSeconds(timeoutInSeconds), TimeSpan.FromMilliseconds(-1));
#else
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
            tokenSource.Token.Register(() => task.TrySetResult(result));
#endif
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
		/// <param name="requestSlotData">Decides if the <see cref="LoginSuccessful"/> result will contain any slot data</param>
		/// <returns>
		///     <see cref="T:Archipelago.MultiClient.Net.LoginSuccessful"/> if the connection is succeeded and the server accepted the login attempt.
		///     <see cref="T:Archipelago.MultiClient.Net.LoginFailure"/> if the connection to the server failed in some way.
		/// </returns>
		/// <remarks>
		///     The connect attempt is synchronous and will lock for up to 5 seconds as it attempts to connect to the server. 
		///     Most connections are instantaneous however the timeout is 5 seconds before it returns <see cref="T:Archipelago.MultiClient.Net.LoginFailure"/>.
		/// </remarks>
		public LoginResult TryConnectAndLogin(string game, string name, ItemsHandlingFlags itemsHandlingFlags, 
	        Version version = null, string[] tags = null, string uuid = null, string password = null, bool requestSlotData = true)
        {
#if NET35
            connectionInfo.SetConnectionParameters(game, tags, itemsHandlingFlags, uuid);

            try
            {
				awaitingRoomInfo = true;
                expectingLoginResult = true;
                loginResult = null;

                Socket.Connect();

                var connectedStartedTime = DateTime.UtcNow;
				while (awaitingRoomInfo)
				{
					if (DateTime.UtcNow - connectedStartedTime > TimeSpan.FromSeconds(ArchipelagoConnectionTimeoutInSeconds))
					{
						Socket.Disconnect();

						return new LoginFailure("Connection timed out.");
					}

					Thread.Sleep(25);
				}

				Socket.SendPacket(BuildConnectPacket(name, password, version, requestSlotData));

                connectedStartedTime = DateTime.UtcNow;
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
	            task.Wait(TimeSpan.FromSeconds(ArchipelagoConnectionTimeoutInSeconds));
            }
            catch (AggregateException e)
            {
	            if (e.GetBaseException() is OperationCanceledException)
		            return new LoginFailure("Connection timed out.");

	            return new LoginFailure(e.GetBaseException().Message);
            }

			if (!task.IsCompleted)
				return new LoginFailure("Connection timed out.");
			
			return LoginAsync(game, name, itemsHandlingFlags, version, tags, uuid, password, requestSlotData).Result;
#endif
        }

        ConnectPacket BuildConnectPacket(string name, string password, Version version, bool requestSlotData) =>
	        new ConnectPacket {
		        Game = ConnectionInfo.Game,
		        Name = name,
		        Password = password,
		        Tags = ConnectionInfo.Tags,
		        Uuid = ConnectionInfo.Uuid,
		        Version = version != null ? new NetworkVersion(version) : new NetworkVersion(0, 4, 0),
		        ItemsHandling = ConnectionInfo.ItemsHandlingFlags,
				RequestSlotData = requestSlotData
			};
    }
}