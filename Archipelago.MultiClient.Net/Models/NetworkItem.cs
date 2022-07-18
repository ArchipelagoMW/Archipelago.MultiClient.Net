using Archipelago.MultiClient.Net.Enums;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Models
{
    public struct NetworkItem
    {
        [JsonProperty("item")]
        public long Item { get; set; }

        [JsonProperty("location")]
        public long Location { get; set; }

        [JsonProperty("player")]
        public int Player { get; set; }

        [JsonProperty("flags")]
        public ItemFlags Flags { get; set; }
    }
}
