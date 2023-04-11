using Archipelago.MultiClient.Net.MessageLog.Parts;
using System.Text;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class LogMessage
	{
		public MessagePart[] Parts { get; }

		internal LogMessage(MessagePart[] parts)
		{
			Parts = parts;
		}

		public override string ToString()
		{
			if (Parts.Length == 1)
				return Parts[0].Text;

			var builder = new StringBuilder(Parts.Length);

			foreach (var part in Parts)
				builder.Append(part.Text);

			return builder.ToString();
		}
	}
}