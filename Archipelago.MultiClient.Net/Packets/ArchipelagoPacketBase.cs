using Archipelago.MultiClient.Net.Enums;
using System;

#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
#endif

namespace Archipelago.MultiClient.Net.Packets
{
	[Serializable]
	public abstract class ArchipelagoPacketBase
	{
		[JsonIgnore]
		internal JObject jobject;

		[JsonProperty("cmd")]
#if NET6_0_OR_GREATER
		[JsonConverter(typeof(JsonStringEnumConverter))]
#else
		[JsonConverter(typeof(StringEnumConverter))]
#endif
		public abstract ArchipelagoPacketType PacketType { get; }

		/// <summary>
		/// Retreive the basic jobject that was send by the server.
		/// Its not recommended to use this however the JObject allows accessing properties are not available in the library
		/// </summary>
		/// <returns></returns>
		public JObject ToJObject() => jobject;
	}
}
