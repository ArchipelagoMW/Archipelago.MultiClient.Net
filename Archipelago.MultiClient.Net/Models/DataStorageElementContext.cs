using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json.Linq;
using System;

namespace Archipelago.MultiClient.Net.Models
{
    class DataStorageElementContext
    {
        internal string Key { get; set; }

        internal Action<string, DataStorageHelper.DataStorageUpdatedHandler> AddHandler { get; set; }
        internal Action<string, DataStorageHelper.DataStorageUpdatedHandler> RemoveHandler { get; set; }
        internal Func<string, JToken> GetData { get; set; }
        internal Action<string, decimal, Action<decimal, decimal>> Deplete { get; set; }

        public override string ToString()
        {
            return $"Key: {Key}";
        }
    }
}