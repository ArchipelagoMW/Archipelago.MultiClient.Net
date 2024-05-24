using System;

namespace Archipelago.MultiClient.Net.Enums
{

	/// <summary>
	/// The types of slots that can be present in a game.
	///	</summary>
	[Flags]
	public enum SlotType
    {
		/// <summary>
		/// A spectator slot. (wont be allowed to send hints or items)
		/// </summary>
        Spectator = 0,
		/// <summary>
		/// A normal player slot
		/// </summary>
		Player = 1 << 0,
		/// <summary>
		/// A slot that represents a group of players, used for itemlinks
		/// </summary>
		Group = 1 << 1
    }
}