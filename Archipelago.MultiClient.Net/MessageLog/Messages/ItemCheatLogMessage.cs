using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An item cheat message to display to the user, consisting of an array of message parts to form a sentence.
	/// Item cheat messages contain additional information about the item that was in response to an `!getitem` command
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.ItemSendLogMessage"/>
	public class ItemCheatLogMessage : ItemSendLogMessage
	{
		internal ItemCheatLogMessage(MessagePart[] parts, 
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, 
			int team, int slot, NetworkItem item)
			: base(parts, players, connectionInfo, slot, 0, item, team)
		{
		}
	}
}