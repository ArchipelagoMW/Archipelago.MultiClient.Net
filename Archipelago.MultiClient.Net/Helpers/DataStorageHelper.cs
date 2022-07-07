using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json.Linq;
#else
using Newtonsoft.Json.Linq;
#endif
using System;
using System.Collections.Generic;
using System.Threading;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class DataStorageHelper
    {
        public delegate void DataStorageUpdatedHandler(JToken originalValue, JToken newValue);

        private readonly Dictionary<string, DataStorageUpdatedHandler> onValueChangedEventHandlers = new Dictionary<string, DataStorageUpdatedHandler>();
        private readonly Dictionary<Guid, DataStorageUpdatedHandler> operationSpecificCallbacks = new Dictionary<Guid, DataStorageUpdatedHandler>();
        private readonly Dictionary<string, Action<JToken>> asyncRetrievalCallbacks = new Dictionary<string, Action<JToken>>();

        private readonly IArchipelagoSocketHelper socket;
        private readonly IConnectionInfoProvider connectionInfoProvider;

        internal DataStorageHelper(IArchipelagoSocketHelper socket, IConnectionInfoProvider connectionInfoProvider)
        {
            this.socket = socket;
            this.connectionInfoProvider = connectionInfoProvider;

            socket.PacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case RetrievedPacket retrievedPacket:
                    foreach (var data in retrievedPacket.Data)
                    {
                        if (asyncRetrievalCallbacks.TryGetValue(data.Key, out var asyncCallback))
                        {
                            asyncCallback(data.Value);

                            asyncRetrievalCallbacks.Remove(data.Key);
                        }
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
                    {
                        handler(setReplyPacket.OriginalValue, setReplyPacket.Value);
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

        private void GetAsync(string key, Action<JToken> callback)
        {
            if (!asyncRetrievalCallbacks.ContainsKey(key))
            {
                asyncRetrievalCallbacks[key] = callback;
            }
            else
            {
                asyncRetrievalCallbacks[key] += callback;
            }

            socket.SendPacketAsync(new GetPacket { Keys = new[] { key } });
        }

        private void Initialize(string key, JToken value)
        {
            socket.SendPacketAsync(new SetPacket
            {
                Key = key,
                DefaultValue = value,
                Operations = new[] {
#if USE_OCULUS_NEWTONSOFT
                    new OperationSpecification { Operation = Operation.Default.ToString() }
#else
                    new OperationSpecification { Operation = Operation.Default }
#endif
                }
            });
        }

        private JToken GetValue(string key)
        {
            JToken value = null;

            GetAsync(key, v => value = v);

            int iterations = 0;
            while (value == null)
            {
                Thread.Sleep(100);
                if (++iterations > 10)
                {
                    throw new TimeoutException($"Timed out retrieving data for key `{key}`. " +
                        $"This may be due to an attempt to retrieve a value from the DataStorageHelper in a synchronous fashion from within a PacketReceived handler. " +
                        $"When using the DataStorageHelper from within code which runs on the websocket thread then use the asynchronous getters. Ex: `DataStorageHelper[\"{key}\"].GetAsync(x => {{}});`" +
                        $"Be aware that DataStorageHelper calls tend to cause packet responses, so making a call from within a PacketReceived handler may cause an infinite loop.");
                }
            }

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
#if USE_OCULUS_NEWTONSOFT
                    Operation = "Replace",
#else
                    Operation = Operation.Replace,
#endif
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
                GetAsync = GetAsync,
                Initialize = Initialize,
                AddHandler = AddHandler,
                RemoveHandler = RemoveHandler
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
                    return $"{scope}:{connectionInfoProvider.Game}:{key}";
                case Scope.Team:
                    return $"{scope}:{connectionInfoProvider.Team}:{key}";
                case Scope.Slot:
                    return $"{scope}:{connectionInfoProvider.Slot}:{key}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, $"Invalid scope for key {key}");
            }
        }
    }
}
