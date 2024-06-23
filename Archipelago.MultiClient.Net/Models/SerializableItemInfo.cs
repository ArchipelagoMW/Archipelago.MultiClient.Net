using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using System.Runtime.Serialization;

#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif

namespace Archipelago.MultiClient.Net.Models
{
	/// <summary>
	/// The minimal information needed to deserialize an ItemInfo given the context of an active archipelago session
	/// </summary>
	public class MinimalSerializableItemInfo
	{
		/// <summary>
		/// The item id of the item. Item ids are in the range of ± (2^53)-1.
		/// </summary>
		public long ItemId { get; set; }

		/// <summary>
		/// The location id of the item inside the world. Location ids are in the range of ± (2^53)-1.
		/// </summary>
		public long LocationId { get; set; }

		/// <summary>
		/// The player slot
		/// </summary>
		public int PlayerSlot { get; set; }

		/// <summary>
		/// Enum flags that describe special properties of the Item
		/// </summary>
		public ItemFlags Flags { get; set; }

		/// <summary>
		/// The game the item belongs to
		/// </summary>
		public string ItemGame { get; set; }

		/// <summary>
		/// The game the location belongs to
		/// </summary>
		public string LocationGame { get; set; }
	}

	/// <summary>
	/// An Json Serializable version of an ItemInfo
	/// </summary>
	public class SerializableItemInfo : MinimalSerializableItemInfo
	{
		/// <summary>
		/// Whether or not this ItemInfo came from an location scout
		/// </summary>
		public bool IsScout { get; set; }

		/// <summary>
		/// The player of the world the item is located in, unless IsScout is true than its the player who will receive the item
		/// </summary>
		public PlayerInfo Player { get; set; }

		/// <summary>
		/// The name of the item, null if the name cannot be resolved
		/// </summary>
		public string ItemName { get; set; }

		/// <summary>
		/// The name of the location that item is at, null if the name cannot be resolved
		/// </summary>
		public string LocationName { get; set; }
		
		/// <summary>
		/// The name of the item for display purposes, returns a fallback string containing the ItemId if the name cannot be resolved 
		/// </summary>
		[JsonIgnore]
		public string ItemDisplayName => ItemName ?? $"Item: {ItemId}";

		/// <summary>
		/// The name of the location for display purposes, returns a fallback string containing the LocationId if the name cannot be resolved 
		/// </summary>
		[JsonIgnore]
		public string LocationDisplayName => LocationName ?? $"Location: {LocationId}";

		/// <summary>
		/// Converts the `SerializableItemInfo` into an json string, either the full length context or a compact context can be preserved
		/// If the json string contained the full context, then it can be deserialized even when no Archipelago session is provided
		/// If the json string contained the minimal context, then during deserialization an active archipelago session must be provided for additional context
		/// </summary>
		/// <param name="full">Should the json contain the full lengthy context, or just the minimal info to reconstruct it later</param>
		/// <returns>The json representation of this ItemInfo</returns>
		public string ToJson(bool full = false)
		{
			MinimalSerializableItemInfo objectToSerialize = this;

			if (!full)
			{
				objectToSerialize = new MinimalSerializableItemInfo {
					ItemId = ItemId,
					LocationId = LocationId,
					PlayerSlot = PlayerSlot,
					Flags = Flags,
				};

				if (IsScout)
					objectToSerialize.ItemGame = ItemGame;
				else
					objectToSerialize.LocationGame = LocationGame;
			}

			var serializerSettings = new JsonSerializerSettings {
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.None
			};
			return JsonConvert.SerializeObject(objectToSerialize, serializerSettings);
		}

		/// <summary>
		/// Converts the json string back to an `SerializableItemInfo`
		/// If the json string contained the full context, than the optional ArchipelagoSession can be left empty
		/// If the json string contained the minimal context, the optional ArchipelagoSession should be provided else context will be missing
		/// </summary>
		/// <param name="json">the json string to input</param>
		/// <param name="session">an reference to an active archipelago session to retrieve contexts such as items names and player data</param>
		/// <returns>The deserialized ItemInfo</returns>
		public static SerializableItemInfo FromJson(string json, IArchipelagoSession session = null)
		{
			var streamingContext = 
				session != null
					? new ItemInfoStreamingContext {
						Items = session.Items,
						Locations = session.Locations,
						PlayerHelper = session.Players,
						ConnectionInfo = session.ConnectionInfo
					}
					: null;

			var serializerSettings = new JsonSerializerSettings
			{
				Context = new StreamingContext(StreamingContextStates.Other, streamingContext)
			};

			return JsonConvert.DeserializeObject<SerializableItemInfo>(json, serializerSettings);
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext streamingContext)
		{
			if (ItemGame == null && LocationGame != null)
				IsScout = false;
			else if (ItemGame != null && LocationGame == null)
				IsScout = true;

			var context = streamingContext.Context as ItemInfoStreamingContext;
			if (context == null)
				return;

			if (IsScout && LocationGame == null)
				LocationGame = context.ConnectionInfo.Game;
			else if (!IsScout && ItemGame == null)
				ItemGame = context.ConnectionInfo.Game;

			if (ItemName == null)
				ItemName = context.Items.GetItemName(ItemId, ItemGame);
			if (LocationName == null)
				LocationName = context.Locations.GetLocationNameFromId(LocationId, LocationGame);
			if (Player == null)
				Player = context.PlayerHelper.GetPlayerInfo(PlayerSlot);
		}
	}

	class ItemInfoStreamingContext
	{
		public IReceivedItemsHelper Items { get; set; }
		public ILocationCheckHelper Locations { get; set; }
		public IPlayerHelper PlayerHelper { get; set; }
		public IConnectionInfoProvider ConnectionInfo { get; set; }
	}
}
