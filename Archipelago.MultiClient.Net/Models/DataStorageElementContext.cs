using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json.Linq;
using System;

namespace Archipelago.MultiClient.Net.Models
{
    class DataStorageElementContext
    {
        public string Key { get; set; }

        public Action<string, DataStorageHelper.DataStorageUpdatedHandler> AddHandler { get; set; }
        public Action<string, DataStorageHelper.DataStorageUpdatedHandler> RemoveHandler { get; set; }
        public Func<string, JToken> GetData { get; set; }
        public Action<string, decimal, Action<decimal, decimal>> Deplete { get; set; }

        public override string ToString()
        {
            return $"Key: {Key}";
        }
    }
}