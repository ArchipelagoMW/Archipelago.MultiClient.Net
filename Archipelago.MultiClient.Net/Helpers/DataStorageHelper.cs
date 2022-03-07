using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class DataStorageHelper
    {
        public delegate void DataStorageUpdatedHandler(JToken originalValue, JToken newValue);

        private readonly Dictionary<string, DataStorageUpdatedHandler> onValueChangedEventHandlers = new Dictionary<string, DataStorageUpdatedHandler>();
        private readonly Dictionary<string, JToken> retrievalResults = new Dictionary<string, JToken>();
        private readonly Dictionary<string, List<Action<JToken>>> asyncRetrievalCallbacks = new Dictionary<string, List<Action<JToken>>>();
        private readonly Dictionary<Guid, DataStorageUpdatedHandler> operationSpecificCallbacks = new Dictionary<Guid, DataStorageUpdatedHandler>();

        private readonly object asyncRetrievalCallbackLockObject = new object();

        private readonly IArchipelagoSocketHelper socket;

        private int slot;
        private int team;

        internal DataStorageHelper(IArchipelagoSocketHelper socket)
        {
            this.socket = socket;

            socket.PacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case ConnectedPacket connectedPacket:
                    slot = connectedPacket.Slot;
                    team = connectedPacket.Team;
                    break;
                case RetrievedPacket retrievedPacket:
                    foreach (var data in retrievedPacket.Data)
                    {
                        retrievalResults[data.Key] = data.Value;

                        lock (asyncRetrievalCallbackLockObject)
                        {
                            if (asyncRetrievalCallbacks.TryGetValue(data.Key, out var callbacks))
                            {
                                foreach (var callback in callbacks)
                                {
                                    callback(data.Value);
                                }

                                asyncRetrievalCallbacks.Remove(data.Key);
                            }
                        }
                    }
                    break;
                case SetReplyPacket setReplyPacket:
                    if (onValueChangedEventHandlers.TryGetValue(setReplyPacket.Key, out var handler))
                    {
                        handler(setReplyPacket.OriginalValue, setReplyPacket.Value);
                    }

                    if (setReplyPacket.Reference.HasValue 
                        && operationSpecificCallbacks.TryGetValue(setReplyPacket.Reference.Value, out var operationCallback))
                    {
                        operationCallback(setReplyPacket.OriginalValue, setReplyPacket.Value);

                        operationSpecificCallbacks.Remove(setReplyPacket.Reference.Value);
                    }
                    break;
            }
        }

        /// <summary>
        /// Stored or retrieves a value from the server side data storage.
        /// Assignment operations like = or *= will run asynchronously on server only
        /// The retrievals are done synchronously however this storage is implemented using deferred execution.
        /// </summary>
        /// <param name="scope">The scope of the key</param>
        /// <param name="key">The key for which the value should be stored or retrieved</param>
        /// <returns>A value </returns>
        public DataStorageElement this[Scope scope, string key]
        {
            get => this[AddScope(scope, key)];
            set => this[AddScope(scope, key)] = value;
        }
        /// <summary>
        /// Stored or retrieves a value from the server side data storage.
        /// Assignment operations like = or *= will run asynchronously on server only
        /// The retrievals are done synchronously however this storage is implemented using deferred execution.
        /// </summary>
        /// <param name="key">The key under which the value should be stored or retrieved</param>
        /// <returns>A value </returns>
        public DataStorageElement this[string key]
        {
            get => new DataStorageElement(GetContextForKey(key));
            set => SetValue(key, value);
        }

        /// <summary>
        /// Retrieves the value of a certain key from server side data storage.
        /// </summary>
        /// <param name="scope">The scope of the key, the default scope is global</param>
        /// <param name="key">The key for which the value should retrieved</param>
        /// <param name="callback">The callback that will be called when the value is retrieved</param>
        public void GetAsync<T>(Scope scope, string key, Action<T> callback)
        {
            GetAsync(AddScope(scope, key), t => callback(t.ToObject<T>()));
        }
        /// <summary>
        /// Retrieves the value of a certain key from server side data storage.
        /// </summary>
        /// <param name="scope">The scope of the key, the default scope is global</param>
        /// <param name="key">The key for which the value should retrieved</param>
        /// <param name="callback">The callback that will be called when the value is retrieved</param>
        public void GetAsync(Scope scope, string key, Action<JToken> callback)
        {
            GetAsync(AddScope(scope, key), callback);
        }
        /// <summary>
        /// Retrieves the value of a certain key from server side data storage.
        /// </summary>
        /// <param name="key">The key for which the value should retrieved</param>
        /// <param name="callback">The callback that will be called when the value is retrieved</param>
        public void GetAsync<T>(string key, Action<T> callback)
        {
            GetAsync(key, t => callback(t.ToObject<T>()));
        }
        /// <summary>
        /// Retrieves the value of a certain key from server side data storage.
        /// </summary>
        /// <param name="key">The key for which the value should retrieved</param>
        /// <param name="callback">The callback that will be called when the value is retrieved</param>
        public void GetAsync(string key, Action<JToken> callback)
        {
            lock (asyncRetrievalCallbackLockObject)
            {
                if (!asyncRetrievalCallbacks.ContainsKey(key))
                {
                    asyncRetrievalCallbacks[key] = new List<Action<JToken>> { callback };
                }
                else
                {
                    asyncRetrievalCallbacks[key].Add(callback);
                }
            }

            socket.SendPacketAsync(new GetPacket { Keys = new[] { key } });
        }
        
        /// <summary>
        /// Initializes a value in the server side data storage
        /// Will not override any existing value, only set the default value if none existed
        /// </summary>
        /// <param name="scope">The scope of the key, the default scope is global</param>
        /// <param name="key">The key for which the value should retrieved</param>
        /// <param name="value">The default value for the key</param>
        public void Initialize(Scope scope, string key, IEnumerable value)
        {
            Initialize(AddScope(scope, key), value);
        }
        /// <summary>
        /// Initializes a value in the server side data storage
        /// Will not override any existing value, only set the default value if none existed
        /// </summary>
        /// <param name="key">The key for which the value should retrieved</param>
        /// <param name="value">The default value for the key</param>
        public void Initialize(string key, IEnumerable value)
        {
            Initialize(key, JArray.FromObject(value));
        }
        /// <summary>
        /// Initializes a value in the server side data storage
        /// Will not override any existing value, only set the default value if none existed
        /// </summary>
        /// <param name="scope">The scope of the key, the default scope is global</param>
        /// <param name="key">The key for which the value should retrieved</param>
        /// <param name="value">The default value for the key</param>
        public void Initialize(Scope scope, string key, JToken value)
        {
            Initialize(AddScope(scope, key), value);
        }
        /// <summary>
        /// Initializes a value in the server side data storage
        /// Will not override any existing value, only set the default value if none existed
        /// </summary>
        /// <param name="key">The key for which the value should retrieved</param>
        /// <param name="value">The default value for the key</param>
        public void Initialize(string key, JToken value)
        {
            socket.SendPacketAsync(new SetPacket
            {
                Key = key,
                DefaultValue = value,
                Operations = new[] {
                    new OperationSpecification { Operation = Operation.Default },
                }
            });
        }

        private JToken GetValue(string key)
        {
            socket.SendPacketAsync(new GetPacket { Keys = new[] { key } });

            var startTime = DateTime.Now;

            while (!retrievalResults.ContainsKey(key))
            {
                if (DateTime.Now - startTime > TimeSpan.FromMilliseconds(1000))
                {
                    throw new TimeoutException($"Timeout retrieving value for key: '{key}' exceeded 1000ms.");
                }

                Thread.Sleep(10);
            }

            var value = retrievalResults[key];

            retrievalResults.Remove(key);

            return value;
        }

        private void SetValue(string key, DataStorageElement e)
        {
            if (e.Context == null)
            {
                e.Context = GetContextForKey(key);
            }
            else if (e.Context.Key != key)
            {
                e.Operations.Insert(0, new OperationSpecification
                {
                    Operation = Operation.Replace,
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

        private DataStorageElementContext GetContextForKey(string key)
        {
            return new DataStorageElementContext
            {
                Key = key,
                GetData = GetValue,
                AddHandler = AddHandler,
                RemoveHandler = RemoveHandler,
            };
        }

        private void AddHandler(string key, DataStorageUpdatedHandler handler)
        {
            if (onValueChangedEventHandlers.ContainsKey(key))
            {
                onValueChangedEventHandlers[key] += handler;
            }
            else
            {
                onValueChangedEventHandlers[key] = handler;
            }

            socket.SendPacketAsync(new SetNotifyPacket { Keys = new[] { key } });
        }

        private void RemoveHandler(string key, DataStorageUpdatedHandler handler)
        {
            if (onValueChangedEventHandlers.ContainsKey(key))
            {
                onValueChangedEventHandlers[key] -= handler;

                if (onValueChangedEventHandlers[key] == null)
                {
                    onValueChangedEventHandlers.Remove(key);
                }
            }
        }

        private string AddScope(Scope scope, string key)
        {
            switch (scope)
            {
                case Scope.Global:
                    return key;
                case Scope.Game:
                    return $"{scope}:{ArchipelagoSession.Game}:{key}";
                case Scope.Team:
                    return $"{scope}:{team}:{key}";
                case Scope.Slot:
                    return $"{scope}:{slot}:{key}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, $"Invalid scope for key {key}");
            }
        }
    }
}
