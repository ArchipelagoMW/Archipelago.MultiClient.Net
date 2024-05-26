using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Parts
{
	/// <summary>
	/// Part of a LogMessage that contains information about an location
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.MessagePart"/>
	public class LocationMessagePart : MessagePart
	{
		/// <summary>
		/// The name of the location
		/// </summary>
		public long LocationId { get; }

		/// <summary>
		/// The player that owns the location
		/// </summary>
		public int Player { get; }

		internal LocationMessagePart(IPlayerHelper players, IItemInfoResolver itemInfoResolver, JsonMessagePart part) 
			: base(MessagePartType.Location, part, Color.Green)
		{
			Player = part.Player ?? 0;

			var game = (players.GetPlayerInfo(Player) ?? new PlayerInfo()).Game;

			switch (part.Type)
			{
				case JsonMessagePartType.LocationId:
					LocationId = long.Parse(part.Text);
					Text = itemInfoResolver.GetLocationName(LocationId, game) ?? $"Location: {LocationId}";
					break;
				case JsonMessagePartType.LocationName:
					LocationId = itemInfoResolver.GetLocationId(part.Text, game);
					Text = part.Text;
					break;
			}
		}
	}
}