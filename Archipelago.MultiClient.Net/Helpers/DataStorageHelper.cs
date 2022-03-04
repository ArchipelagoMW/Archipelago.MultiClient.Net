using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Archipelago.MultiClient.Net.Helpers
{
    public class DataStorageHelper
    {
        public delegate void DataStorageUpdatedHandler(object originalValue, object newValue);

        private readonly Dictionary<string, DataStorageUpdatedHandler> onValueChangedEventHandlers = new Dictionary<string, DataStorageUpdatedHandler>();

        private readonly Dictionary<string, object> retrievalResults = new Dictionary<string, object>();

        private IArchipelagoSocketHelper socket;

        public DataStorageHelper(IArchipelagoSocketHelper socket)
        {
            this.socket = socket;

            socket.PacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case RetrievedPacket retrievedPacket:
                    foreach (var data in retrievedPacket.Data)
                    {
                        retrievalResults[data.Key] = data.Value;
                    }
                    break;
                case SetReplyPacket setReplyPacket:
                    if (onValueChangedEventHandlers.TryGetValue(setReplyPacket.Key, out var handler))
                    {
                        handler(setReplyPacket.OriginalValue, setReplyPacket.Value);
                    }
                    break;
            }
        }

        public DataStorageElement this[string key]
        {
            get => new DataStorageElement(new DataStorageElementContext {
                Key = key, 
                GetData = GetValue,
                AddHandler = AddHandler,
                RemoveHandler = RemoveHandler
            });
            set => SetValue(key, value);
        }

        //TODO move to element
        public void Deplete(string key, DataStorageElement value/*, todo add callback to see how much was depleted*/)
        {
            //requires custom code
        }

        //TODO move to element
        public void GetValueAsync(Action<string, object> onValueRetrieved, params string[] keys)
        {

        }

        object GetValue(string key)
        {
            socket.SendPacketAsync(new GetPacket { Keys = new []{ key } });

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

        void SetValue(string key, DataStorageElement e)
        {
            socket.SendPacketAsync(new SetPacket {
                Key = key,
                Operation = new []{ new OperationSpecification {
                    Operation = e.Operation, 
                    Value = e.Value
                }}
            });
        }

        void AddHandler(string key, DataStorageUpdatedHandler handler)
        {
            if (onValueChangedEventHandlers.ContainsKey(key))
            {
                onValueChangedEventHandlers[key] += handler;
            }
            else
            {
                onValueChangedEventHandlers[key] = handler;
            }

            socket.SendPacketAsync(new SetNotifyPacket { Keys = new []{ key } });
        }

        void RemoveHandler(string key, DataStorageUpdatedHandler handler)
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
    }
}
