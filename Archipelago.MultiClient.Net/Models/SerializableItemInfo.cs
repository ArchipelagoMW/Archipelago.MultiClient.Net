using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;


namespace Archipelago.MultiClient.Net.Models
{
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

		public string ToJson(bool full)
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


		public static SerializableItemInfo FromJson(string json, IArchipelagoSession session = null)
		{
			if (session == null)
				return JsonConvert.DeserializeObject<SerializableItemInfo>(json);

			var streamingContext = new ItemInfoStreamingContext {
				Items = session.Items,
				Locations = session.Locations,
				PlayerHelper = session.Players,
				ConnectionInfo = session.ConnectionInfo
			};

			var serializerSettings = new JsonSerializerSettings
			{
				Context = new StreamingContext(StreamingContextStates.Other, streamingContext)
			};

			return JsonConvert.DeserializeObject<SerializableItemInfo>(json, serializerSettings);
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext streamingContext)
		{
			var context = streamingContext.Context as ItemInfoStreamingContext;
			if (context == null)
				return;

			if (ItemGame == null && LocationGame != null)
			{
				IsScout = false;
				ItemGame = context.ConnectionInfo.Game;
			}
			else if (ItemGame != null && LocationGame == null)
			{
				IsScout = true;
				LocationGame = context.ConnectionInfo.Game;
			}

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
