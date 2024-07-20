using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An join message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `JoinLogMessage` is send in response to a client joining the multiworld
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.PlayerSpecificLogMessage"/>
	public class JoinLogMessage : PlayerSpecificLogMessage
	{
		/// <summary>
		/// The tags used by the client, tags are used for certain functionality that is shared across the mutliworld (e.g. DeathLink)
		/// </summary>
		public string[] Tags { get; }

		internal JoinLogMessage(MessagePart[] parts,
			IPlayerHelper players, int team, int slot, string[] tags)
			: base(parts, players, team, slot)
		{
			Tags = tags;
		}
	}
}