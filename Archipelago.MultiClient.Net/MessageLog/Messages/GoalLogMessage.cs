using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An goal message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `GoalLogMessage` is send in response to a client completing their goal
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.PlayerSpecificLogMessage"/>
	public class GoalLogMessage : PlayerSpecificLogMessage
	{
		internal GoalLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot)
			: base(parts, players, connectionInfo, team, slot)
		{
		}
	}
}