using System;

namespace Archipelago.MultiClient.Net.Enums
{
	/// <summary>
	/// Enum flags that describe special properties of the Item
	/// </summary>
	[Flags]
    public enum ItemFlags
    {
		/// <summary>
		/// Nothing special about this item
		/// </summary>
        None = 0,
		/// <summary>
		/// Indicates the item can unlock logical advancement
		/// </summary>
		Advancement = 1 << 0,
		/// <summary>
		/// Indicates the item is important but not in a way that unlocks advancement
		/// </summary>
		NeverExclude = 1 << 1,
		/// <summary>
		/// Indicates the item is a trap
		/// </summary>
		Trap = 1 << 2,
    }
}
