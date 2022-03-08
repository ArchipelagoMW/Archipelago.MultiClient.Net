namespace Archipelago.MultiClient.Net.Enums
{
    public enum Scope
    {
        /// <summary>
        /// Keys in this scope are shared with all clients
        /// </summary>
        Global,

        /// <summary>
        /// Keys in this scope are with shared with clients that connect under the same game as you connected with
        /// </summary>
        Game,

        /// <summary>
        /// Keys in this scope are with shared with clients on your team
        /// </summary>
        Team,

        /// <summary>
        /// Keys in this scope are with shared with clients connected to your slot
        /// </summary>
        Slot,
    }
}
