using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Models
{
	/// <summary>
	/// Item information
	/// </summary>
    public struct NetworkItem
    {
		/// <summary>
		/// The item id of the item. Item ids are in the range of ± (2^53)-1.
		/// </summary>
		[JsonProperty("item")]
        public long Item { get; set; }

		/// <summary>
		/// The location id of the item inside the world. Location ids are in the range of ± (2^53)-1.
		/// </summary>
		[JsonProperty("location")]
        public long Location { get; set; }

		/// <summary>
		/// The player slot of the world the item is located in, except when inside an LocationInfo Packet then it will be the slot of the player to receive the item
		/// </summary>
		[JsonProperty("player")]
        public int Player { get; set; }

		/// <summary>
		/// Enum flags that describe special properties of the Item
		/// </summary>
        [JsonProperty("flags")]
        public ItemFlags Flags { get; set; }
    }
}
