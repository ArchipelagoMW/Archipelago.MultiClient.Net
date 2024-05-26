namespace Archipelago.MultiClient.Net.Enums
{
	/// <summary>
	/// Specifies if a location scout should create an actual hint on the AP server
	/// Hints created using location scouts never cost hintpoints
	/// </summary>
	public enum HintCreationPolicy
	{
		/// <summary>
		/// Do not create a hint
		/// </summary>
		None = 0,
		/// <summary>
		/// Create a hint and announce it player that the item belongs to
		/// </summary>
		CreateAndAnnounce = 1,
		/// <summary>
		/// Create a hint and announce it player that the item belongs to but only once announce if the hint didnt exist yet
		/// </summary>
		CreateAndAnnounceOnce = 2,
	}
}
