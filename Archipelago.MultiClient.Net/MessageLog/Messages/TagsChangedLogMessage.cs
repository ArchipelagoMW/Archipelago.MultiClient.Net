using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An tags changed message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `TagsChangedLogMessage` is send in response to a client changing their tags
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.PlayerSpecificLogMessage"/>
	public class TagsChangedLogMessage : PlayerSpecificLogMessage
	{
		/// <summary>
		/// The tags used by the client, tags are used for certain functionality that is shared across the mutliworld (e.g. DeathLink)
		/// </summary>
		public string[] Tags { get; }

		internal TagsChangedLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot, string[] tags)
			: base(parts, players, connectionInfo, team, slot)
		{
			Tags = tags;
		}
	}
}