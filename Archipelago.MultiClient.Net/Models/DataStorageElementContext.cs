using Archipelago.MultiClient.Net.Helpers;
using System;

namespace Archipelago.MultiClient.Net.Models
{
    class DataStorageElementContext
    {
        public Action<string, DataStorageHelper.DataStorageUpdatedHandler> AddHandler { get; set; }
        public Action<string, DataStorageHelper.DataStorageUpdatedHandler> RemoveHandler { get; set; }
        public Func<string, object> GetData { get; set; }
        public string Key { get; set; }
    }
}