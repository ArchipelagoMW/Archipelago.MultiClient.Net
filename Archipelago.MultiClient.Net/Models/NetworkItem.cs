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
        public int Item { get; set; }

        [JsonProperty("location")]
        public int Location { get; set; }

        [JsonProperty("player")]
        public int Player { get; set; }
    }
}
