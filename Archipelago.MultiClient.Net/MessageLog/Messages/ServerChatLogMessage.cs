using Archipelago.MultiClient.Net.MessageLog.Parts;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An chat message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `ServerChatLogMessage` is send in response to the server chatting some message
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.PlayerSpecificLogMessage"/>
	public class ServerChatLogMessage : LogMessage
	{
		/// <summary>
		/// The message that was chatted
		/// </summary>
		public string Message { get; }

		internal ServerChatLogMessage(MessagePart[] parts, string message) : base(parts)
		{
			Message = message;
		}
	}
}