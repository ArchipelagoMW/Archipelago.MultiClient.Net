using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Archipelago.MultiClient.Net.Helpers
{
    class DataStorageHelper
    {
        public delegate void DataStorageUpdatedHandler(string key, object originalValue, object newValue);

        private readonly EventHandlerList onValueChangedEventHandlers = new EventHandlerList();
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
                    var handler = (DataStorageUpdatedHandler)onValueChangedEventHandlers[setReplyPacket.Key];
                    if (handler != null)
                    {
                        handler(setReplyPacket.Key, setReplyPacket.OriginalValue, setReplyPacket.Value);
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

        public void Deplete(string key, DataStorageElement value)
        {
            value.Method = "deplete";
            this[key] = value;
        }

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
                Value = e.Value,
                Operation = e.Method ?? "replace"
            });
        }

        void AddHandler(string key, DataStorageUpdatedHandler handler)
        {
            onValueChangedEventHandlers.AddHandler(key, handler);

            socket.SendPacketAsync(new SetNotifyPacket { Keys = new []{ key } });
        }

        void RemoveHandler(string key, DataStorageUpdatedHandler handler)
        {
            onValueChangedEventHandlers.RemoveHandler(key, handler);
        }
    }
}
