using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;

namespace Archipelago.MultiClient.Net.MessageLog.Messages
{
	/// <summary>
	/// An item hint message to display to the user, consisting of an array of message parts to form a sentence.
	/// The `HintItemSendLogMessage` is send in response to a client hinting for an item.
	/// Item hint messages contain additional information about the item that was sent for more specific processing
	/// </summary>
	/// <seealso cref="T:Archipelago.MultiClient.Net.MessageLog.Messages.ItemSendLogMessage"/>
	public class HintItemSendLogMessage : ItemSendLogMessage
	{
		/// <summary>
		/// Indicates if the location of this item was already checked
		/// </summary>
		public bool IsFound { get; }

		internal HintItemSendLogMessage(MessagePart[] parts, IPlayerHelper players, 
			int receiver, int sender, NetworkItem item, bool found, IItemInfoResolver itemInfoResolver) 
			: base(parts, players, receiver, sender, item, itemInfoResolver)
		{
			IsFound = found;
		}
	}
}