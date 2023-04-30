using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An leave message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `LeaveLogMessage` is send in response to a client disconnecting from the multiworld
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.PlayerSpecificLogMessage"/>
	public class LeaveLogMessage : PlayerSpecificLogMessage
	{
		internal LeaveLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot)
			: base(parts, players, connectionInfo, team, slot)
		{
		}
	}
}