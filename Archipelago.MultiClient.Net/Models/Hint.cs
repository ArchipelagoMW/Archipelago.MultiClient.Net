using Archipelago.MultiClient.Net.Enums;

#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Models
{
	public class Hint
	{
		[JsonProperty("receiving_player")]
		public int ReceivingPlayer { get; set; }

		[JsonProperty("finding_player")]
		public int FindingPlayer { get; set; }

		[JsonProperty("item")]
		public long ItemId { get; set; }

		[JsonProperty("location")]
		public long LocationId { get; set; }

		[JsonProperty("item_flags")]
		public ItemFlags ItemFlags { get; set; }

		[JsonProperty("found")]
		public bool Found { get; set; }

		[JsonProperty("entrance")]
		public string Entrance { get; set; }
	}
}
