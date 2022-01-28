using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archipelago.MultiClient.Net.Enums
{
    /// <summary>
    /// Indicates to the server how the client would like to get ReceivedItems packets.
    /// </summary>
    [Flags]
    public enum ItemsHandlingFlags
    {
        /// <summary>
        /// Don't get any ReceivedItems packets from the server.
        /// </summary>
        NoItems = 0,

        /// <summary>
        /// Get ReceivedItems packets for items being granted to you by others.
        /// </summary>
        RemoteItems = 1,

        /// <summary>
        /// Get ReceivedItems packets for items which you picked up in your own world. Automatically includes RemoteItems option as it is required to set this.
        /// </summary>
        IncludeOwnItems = RemoteItems | 1 << 1,

        /// <summary>
        /// Get ReceivedItems packets for your starting inventory. Automatically includes RemoteItems option as it is required to set this.
        /// </summary>
        IncludeStartingInventory = RemoteItems | 1 << 2,

        /// <summary>
        /// Get ReceivedItems packets for all remote items, items in your world, and starting inventory granted to you.
        /// </summary>
        AllItems = RemoteItems | IncludeOwnItems | IncludeStartingInventory
    }
}
