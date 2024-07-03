using Archipelago.MultiClient.Net.Helpers;
using System;

#if !NET35
using System.Threading.Tasks;
#endif

#if NET6_0_OR_GREATER
using JToken = System.Text.Json.Nodes.JsonNode;
#else
using Newtonsoft.Json.Linq;
#endif

namespace Archipelago.MultiClient.Net.Models
{
    class DataStorageElementContext
    {
        internal string Key { get; set; }

        internal Action<string, DataStorageHelper.DataStorageUpdatedHandler> AddHandler { get; set; }
        internal Action<string, DataStorageHelper.DataStorageUpdatedHandler> RemoveHandler { get; set; }
        internal Func<string, JToken> GetData { get; set; }
        internal Action<string, JToken> Initialize { get; set; }
#if NET35
        internal Action<string, Action<JToken>> GetAsync { get; set; }
#else
        internal Func<string, Task<JToken>> GetAsync { get; set; }
#endif

        public override string ToString() => $"Key: {Key}";
    }
}