namespace Archipelago.MultiClient.Net.Enums
{
	/// <summary>
	/// Type of message parts of an print message
	/// </summary>
    public enum JsonMessagePartType
    {
        /// <summary>
        /// Regular text content. this is the default type and as such may be omitted.
        /// </summary>
        Text,

        /// <summary>
        /// Player ID of someone on your team, should be resolved to Player Name
        /// </summary>
        PlayerId,

        /// <summary>
        /// Player Name, could be a player within a multiplayer game or from another team, not ID resolvable
        /// </summary>
        PlayerName,

        /// <summary>
        /// Item ID, should be resolved to Item Name
        /// </summary>
        ItemId,

        /// <summary>
        /// Item Name, not currently used over network, but supported by reference Clients.
        /// </summary>
        ItemName,

        /// <summary>
        /// Location ID, should be resolved to Location Name
        /// </summary>
        LocationId,

        /// <summary>
        /// Location Name, not currently used over network, but supported by reference Clients.
        /// </summary>
        LocationName,

        /// <summary>
        /// Entrance Name. No ID mapping exists.
        /// </summary>
        EntranceName,

        /// <summary>
        /// Regular text that should be colored. Only type that will contain color data.
        /// </summary>
        Color
    }
}
