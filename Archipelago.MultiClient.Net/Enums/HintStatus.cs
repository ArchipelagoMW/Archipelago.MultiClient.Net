namespace Archipelago.MultiClient.Net.Enums
{
	/// <summary>
	/// The state of a player
	/// </summary>
    public enum HintStatus : int
    {
		/// <summary>
		/// The location has been collected. Status cannot be changed once found.
		/// </summary>
		Found = 0,
		/// <summary>
		/// The receiving player has not specified any status
		/// </summary>
		Unspecified = 1,
		/// <summary>
		/// The receiving player has specified that the item is unneeded
		/// </summary>
		NoPriority = 10,
		/// <summary>
		/// The receiving player has specified that the item is detrimental
		/// </summary>
		Avoid = 20,
		/// <summary>
		/// The receiving player has specified that the item is needed
		/// </summary>
		Priority = 30
    }
}
