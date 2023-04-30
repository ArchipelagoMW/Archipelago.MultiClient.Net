namespace Archipelago.MultiClient.Net.MessageLog.Parts
{
	/// <summary>
	/// The type of message part
	/// </summary>
	public enum MessagePartType
	{
		/// <inheritdoc cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.MessagePart"/>
		Text,
		/// <inheritdoc cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.PlayerMessagePart"/>
		Player,
		/// <inheritdoc cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.ItemMessagePart"/>
		Item,
		/// <inheritdoc cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.LocationMessagePart"/>
		Location,
		/// <inheritdoc cref="T:Archipelago.MultiClient.Net.MessageLog.Parts.EntranceMessagePart"/>
		Entrance
	}
}