#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Models
{
    public class DataPackage
    {
        [JsonProperty("games")]
        public Dictionary<string, GameData> Games { get; set; } = new Dictionary<string, GameData>();
    }
}
