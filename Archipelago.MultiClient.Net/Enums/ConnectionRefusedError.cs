namespace Archipelago.MultiClient.Net.Enums
{
    public enum ConnectionRefusedError
    {
        /// <summary>
        /// Indicates that the sent 'name' field did not match any auth entry on the server.
        /// </summary>
        InvalidSlot,

        /// <summary>
        /// Indicates that a correctly named slot was found, but the game for it mismatched.
        /// </summary>
        InvalidGame,

        /// <summary>
        /// Indicates a connection with a different uuid is already established.
        /// </summary>
        SlotAlreadyTaken,

        /// <summary>
        /// IncompatibleVersion indicates a version mismatch.
        /// </summary>
        IncompatibleVersion,

        /// <summary>
        /// InvalidPassword indicates the wrong, or no password when it was required, was sent.
        /// </summary>
        InvalidPassword
    }
}
