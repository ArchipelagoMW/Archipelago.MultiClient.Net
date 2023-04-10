using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	public class ItemSendLogMessage : LogMessage
	{
		public int ReceivingPlayerSlot { get; }
		public int SendingPlayerSlot { get; }
		public NetworkItem Item { get; }

		internal ItemSendLogMessage(MessagePart[] parts, int receiver, int sender, NetworkItem item) : base(parts)
		{
			ReceivingPlayerSlot = receiver;
			SendingPlayerSlot = sender;
			Item = item;
		}
	}
}