namespace Archipelago.MultiClient.Net.Enums
{
	/// <summary>
	/// The state of a player
	/// </summary>
    public enum ArchipelagoClientState : int
    {
		/// <summary>
		/// Default value, no state is known for this player
		/// </summary>
        ClientUnknown = 0,
		/// <summary>
		/// The player is connected to the multiworld
		/// </summary>
		ClientConnected = 5,
		/// <summary>
		/// The player is ready to start playing
		/// </summary>
		ClientReady = 10,
		/// <summary>
		/// The player started playing
		/// </summary>
		ClientPlaying = 20,
		/// <summary>
		/// The player has finished their goal
		/// </summary>
		ClientGoal = 30
    }
}
