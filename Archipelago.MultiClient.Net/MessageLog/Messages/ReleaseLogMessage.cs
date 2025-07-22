using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An release message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `ReleaseLogMessage` is send in response to a client releasing their items
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.PlayerSpecificLogMessage"/>
	public class ReleaseLogMessage : PlayerSpecificLogMessage
	{
		internal ReleaseLogMessage(MessagePart[] parts,
			IPlayerHelper players, int team, int slot)
			: base(parts, players, team, slot)
		{
		}
	}
}