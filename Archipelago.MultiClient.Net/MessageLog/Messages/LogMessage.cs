using Archipelago.MultiClient.Net.MessageLog.Parts;
using System.Text;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// A message to display to the user, consisting of an array of message parts to form a sentence
	/// This is the base class for all LogMessage's
	/// </summary>
	public class LogMessage
	{
		/// <summary>
		/// Different part of a message that should be used to build a sentence
		/// The order of the parts is the order the different sections should appear in
		/// </summary>
		public MessagePart[] Parts { get; }

		internal LogMessage(MessagePart[] parts)
		{
			Parts = parts;
		}

		/// <summary>
		/// Uses the Parts to form a correct sentence
		/// </summary>
		/// <returns>the sentence this LogMessage is representing</returns>
		public override string ToString()
		{
			if (Parts.Length == 1)
				return Parts[0].Text;

			var builder = new StringBuilder();

			foreach (var part in Parts)
				builder.Append(part.Text);

			return builder.ToString();
		}
	}
}