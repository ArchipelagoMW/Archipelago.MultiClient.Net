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

		internal LocationMessagePart(ILocationCheckHelper locations, JsonMessagePart part) 
			: base(MessagePartType.Location, part, Color.Green)
		{
			switch (part.Type)
			{
				case JsonMessagePartType.LocationId:
					LocationId = long.Parse(part.Text);
					Text = locations.GetLocationNameFromId(LocationId) ?? $"Location: {LocationId}";
					break;
				case JsonMessagePartType.PlayerName:
					LocationId = 0; // we are not going to try to reverse lookup as we don't know the game this location belongs to
					Text = part.Text;
					break;
			}
		}
	}
}