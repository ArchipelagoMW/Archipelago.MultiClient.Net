using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

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

		[JsonProperty("item_flags")]
		public ItemFlags ItemFlags { get; set; }

		[JsonProperty("found")]
		public long Found { get; set; }

		[JsonProperty("entrance")]
		public string Entrance { get; set; }
	}
}
