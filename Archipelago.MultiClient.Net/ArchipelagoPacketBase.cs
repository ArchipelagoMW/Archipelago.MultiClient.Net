using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

namespace Archipelago.MultiClient.Net
{
    [Serializable]
    public abstract class ArchipelagoPacketBase
    {
	    [JsonIgnore]
		internal JObject jobject;

		[JsonProperty("cmd")]
        [JsonConverter(typeof(StringEnumConverter))]
        public abstract ArchipelagoPacketType PacketType { get; }

        /// <summary>
        /// Retreive the basic jobject that was send by the server.
        /// Its not recommended to use this however the JObject allows accessing properties are not available in the library
        /// </summary>
        /// <returns></returns>
        public JObject ToJObject() => jobject;
    }
}
