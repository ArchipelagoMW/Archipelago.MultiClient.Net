using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An chat message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `ChatLogMessage` is send in response to a client chatting some message
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.PlayerSpecificLogMessage"/>
	public class ChatLogMessage : PlayerSpecificLogMessage
	{
		/// <summary>
		/// The message that was chatted
		/// </summary>
		public string Message { get; }

		internal ChatLogMessage(MessagePart[] parts,
			IPlayerHelper players, IConnectionInfoProvider connectionInfo, int team, int slot, string message) 
			: base(parts, players, connectionInfo, team, slot)
		{
			Message = message;
		}
	}
}