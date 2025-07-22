using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An collect message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `CollectLogMessage` is send in response to a client collecting their items
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.PlayerSpecificLogMessage"/>
	public class CollectLogMessage : PlayerSpecificLogMessage
	{
		internal CollectLogMessage(MessagePart[] parts,
			IPlayerHelper players, int team, int slot) 
			: base(parts, players, team, slot)
		{
		}
	}
}