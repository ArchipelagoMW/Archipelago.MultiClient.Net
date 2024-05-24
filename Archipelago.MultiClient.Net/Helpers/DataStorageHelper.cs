using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

#if NET35
using System.Threading;
#else
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Provides access to a server side data storage to share and store values across sessions and players
	/// </summary>
	public interface IDataStorageHelper : IDataStorageWrapper
	{
		/// <summary>
		/// Stored or retrieves a value from the server side data storage.
		/// Assignment operations like = or *= will run asynchronously on server only
		/// The retrievals are done synchronously however this storage is implemented using deferred execution.
		/// </summary>
		/// <param name="scope">The scope of the key</param>
		/// <param name="key">The key for which the value should be stored or retrieved</param>
		/// <returns>A value </returns>
		DataStorageElement this[Scope scope, string key] { get; set; }

		/// <summary>
		/// Stored or retrieves a value from the server side data storage.
		/// Assignment operations like = or *= will run asynchronously on server only
		/// The retrievals are done synchronously however this storage is implemented using deferred execution.
		/// </summary>
		/// <param name="key">The key under which the value should be stored or retrieved</param>
		/// <returns>A value </returns>
		DataStorageElement this[string key] { get; set; }
	}

	/// <summary>
	/// Provides access to a server side data storage to share and store values across sessions and players
	/// </summary>
	public partial class DataStorageHelper : IDataStorageHelper
	{
		/// <summary>
		/// Delegate for the callback that is called when a value in the data storage is updated
		/// </summary>
		/// <param name="originalValue">The original value before the update</param>
		/// <param name="newValue">the current value</param>
        public delegate void DataStorageUpdatedHandler(JToken originalValue, JToken newValue);

        readonly Dictionary<string, DataStorageUpdatedHandler> onValueChangedEventHandlers = new Dictionary<string, DataStorageUpdatedHandler>();
        readonly Dictionary<Guid, DataStorageUpdatedHandler> operationSpecificCallbacks = new Dictionary<Guid, DataStorageUpdatedHandler>();
#if NET35
        readonly Dictionary<string, Action<JToken>> asyncRetrievalCallbacks = new Dictionary<string, Action<JToken>>();
#else
        readonly Dictionary<string, TaskCompletionSource<JToken>> asyncRetrievalTasks = new Dictionary<string, TaskCompletionSource<JToken>>();
#endif

        readonly IArchipelagoSocketHelper socket;
        readonly IConnectionInfoProvider connectionInfoProvider;

        internal DataStorageHelper(IArchipelagoSocketHelper socket, IConnectionInfoProvider connectionInfoProvider)
        {
            this.socket = socket;
            this.connectionInfoProvider = connectionInfoProvider;

            socket.PacketReceived += OnPacketReceived;
        }

        void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case RetrievedPacket retrievedPacket:
                    foreach (var data in retrievedPacket.Data)
                    {
#if NET35
                        if (asyncRetrievalCallbacks.TryGetValue(data.Key, out var asyncCallback))
                        {
                            asyncCallback(data.Value);

                            asyncRetrievalCallbacks.Remove(data.Key);
                        }
#else
                        if (asyncRetrievalTasks.TryGetValue(data.Key, out var asyncRetrievalTask))
                        {
                            asyncRetrievalTask.TrySetResult(data.Value);

                            asyncRetrievalTasks.Remove(data.Key);
                        }
#endif
                    }
                    break;
                case SetReplyPacket setReplyPacket:
                    if (setReplyPacket.Reference.HasValue
                        && operationSpecificCallbacks.TryGetValue(setReplyPacket.Reference.Value, out var operationCallback))
                    {
                        operationCallback(setReplyPacket.OriginalValue, setReplyPacket.Value);

                        operationSpecificCallbacks.Remove(setReplyPacket.Reference.Value);
                    }

                    if (onValueChangedEventHandlers.TryGetValue(setReplyPacket.Key, out var handler))
                        handler(setReplyPacket.OriginalValue, setReplyPacket.Value);
                    break;
            }
        }

		/// <inheritdoc />
        public DataStorageElement this[Scope scope, string key]
        {
            get => this[AddScope(scope, key)];
            set => this[AddScope(scope, key)] = value;
        }
		/// <inheritdoc />
		public DataStorageElement this[string key]
        {
            get => new DataStorageElement(GetContextForKey(key));
            set => SetValue(key, value);
        }

#if NET35
		void GetAsync(string key, Action<JToken> callback)
        {
            if (!asyncRetrievalCallbacks.ContainsKey(key))
                asyncRetrievalCallbacks[key] = callback;
            else
                asyncRetrievalCallbacks[key] += callback;

            socket.SendPacketAsync(new GetPacket { Keys = new[] { key } });
        }
#else
	    Task<JToken> GetAsync(string key)
        {
            if (asyncRetrievalTasks.TryGetValue(key, out var asyncRetrievalTask))
            {
                return asyncRetrievalTask.Task;
            }
            else
            {
                var newRetrievalTask = new TaskCompletionSource<JToken>();

                asyncRetrievalTasks[key] = newRetrievalTask;

                socket.SendPacketAsync(new GetPacket { Keys = new[] { key } });

                return newRetrievalTask.Task;
            }
        }
#endif

        void Initialize(string key, JToken value) =>
	        socket.SendPacketAsync(new SetPacket
	        {
		        Key = key,
		        DefaultValue = value,
		        Operations = new[] {
			        new OperationSpecification { OperationType = OperationType.Default }
		        }
	        });

        JToken GetValue(string key)
        {
#if NET35
            JToken value = null;

            GetAsync(key, v => value = v);

            int iterations = 0;
            while (value == null)
            {
                Thread.Sleep(10);
                if (++iterations > 200)
                {
                    throw new TimeoutException($"Timed out retrieving data for key `{key}`. " +
                        $"This may be due to an attempt to retrieve a value from the DataStorageHelper in a synchronous fashion from within a PacketReceived handler. " +
                        $"When using the DataStorageHelper from within code which runs on the websocket thread then use the asynchronous getters. Ex: `DataStorageHelper[\"{key}\"].GetAsync(x => {{}});`" +
                        $"Be aware that DataStorageHelper calls tend to cause packet responses, so making a call from within a PacketReceived handler may cause an infinite loop.");
                }
            }
            
            return value;
#else
            var t = GetAsync(key);
            if (!t.Wait(TimeSpan.FromSeconds(2)))
            {
                throw new TimeoutException($"Timed out retrieving data for key `{key}`. " +
                   $"This may be due to an attempt to retrieve a value from the DataStorageHelper in a synchronous fashion from within a PacketReceived handler. " +
                   $"When using the DataStorageHelper from within code which runs on the websocket thread then use the asynchronous getters. Ex: `DataStorageHelper[\"{key}\"].GetAsync().ContinueWith(x => {{}});`" +
                   $"Be aware that DataStorageHelper calls tend to cause packet responses, so making a call from within a PacketReceived handler may cause an infinite loop.");
            }
            return t.Result;
#endif
        }

        void SetValue(string key, DataStorageElement e)
        {
	        if (key.StartsWith("_read_"))
		        throw new InvalidOperationException($"DataStorage write operation on readonly key '{key}' is not allowed");

	        if (e == null)
		        e = new DataStorageElement(OperationType.Replace, JValue.CreateNull());
	        
	        if (e.Context == null)
            {
                e.Context = GetContextForKey(key);
            }
            else if (e.Context.Key != key)
            {
                e.Operations.Insert(0, new OperationSpecification
                {
                    OperationType = OperationType.Replace,
                    Value = GetValue(e.Context.Key)
                });
            }

            if (e.Callbacks != null)
            {
                var guid = Guid.NewGuid();

                operationSpecificCallbacks[guid] = e.Callbacks;

                socket.SendPacketAsync(new SetPacket
                {
                    Key = key,
                    Operations = e.Operations.ToArray(),
                    WantReply = true,
                    Reference = guid
                });
            }
            else
            {
                socket.SendPacketAsync(new SetPacket
                {
                    Key = key,
                    Operations = e.Operations.ToArray()
                });
            }
        }

        DataStorageElementContext GetContextForKey(string key) =>
	        new DataStorageElementContext
	        {
		        Key = key,
		        GetData = GetValue,
		        GetAsync = GetAsync,
		        Initialize = Initialize,
		        AddHandler = AddHandler,
		        RemoveHandler = RemoveHandler
	        };

        void AddHandler(string key, DataStorageUpdatedHandler handler)
        {
            if (onValueChangedEventHandlers.ContainsKey(key))
                onValueChangedEventHandlers[key] += handler;
            else
                onValueChangedEventHandlers[key] = handler;

            socket.SendPacketAsync(new SetNotifyPacket { Keys = new[] { key } });
        }

        void RemoveHandler(string key, DataStorageUpdatedHandler handler)
        {
	        if (!onValueChangedEventHandlers.ContainsKey(key)) 
		        return;

	        onValueChangedEventHandlers[key] -= handler;

	        if (onValueChangedEventHandlers[key] == null)
		        onValueChangedEventHandlers.Remove(key);
        }

        string AddScope(Scope scope, string key)
        {
            switch (scope)
            {
                case Scope.Global:
                    return key;
                case Scope.Game:
                    return $"{scope}:{connectionInfoProvider.Game}:{key}";
                case Scope.Team:
                    return $"{scope}:{connectionInfoProvider.Team}:{key}";
                case Scope.Slot:
                    return $"{scope}:{connectionInfoProvider.Slot}:{key}";
				case Scope.ReadOnly:
					return $"_read_{key}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, $"Invalid scope for key {key}");
            }
        }
    }
}
