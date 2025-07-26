using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System;

namespace Archipelago.MultiClient.Net.Helpers
{
    /// <summary>
    /// Provides information about the current state of the server
    /// </summary>
    public interface ICreateHintsHelper
    {
        /// <summary>
        ///     Tell the server to create hints for the specified locations.
        ///     When creating hints for another slot's locations, the packet will fail if any of those locations do not contain an item for the requesting slot.
        ///     When creating hints for your own slot's locations, non-existing locations will be silently skipped.
        /// </summary>
        /// <param name="player">
        ///     The ID of the player whose locations ar ebeing hinted for.
        /// </param>
        /// <param name="hintStatus">
        ///     If included, sets the status of the created hints to this status.
        ///     Defaults to unspecified.
        /// </param>
        /// <param name="ids">
        ///     The location ids to create hints for.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        void CreateHints(int player, HintStatus hintStatus = HintStatus.Unspecified, params long[] ids);

        /// <summary>
        ///     Tell the server to create hints for the specified locations.
        ///     This version does not include the player id, and as such will only hint for locations in the active player's game.
        ///     When creating hints, non-existing locations will be silently skipped.
        /// </summary>
        /// <param name="hintStatus">
        ///     If included, sets the status of the created hints to this status.
        ///     Defaults to unspecified.
        /// </param>
        /// <param name="ids">
        ///     The location ids to create hints for.
        /// </param>
        /// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
        ///     The websocket connection is not alive.
        /// </exception>
        void CreateHints(HintStatus hintStatus = HintStatus.Unspecified, params long[] ids);
    }

    ///<inheritdoc/>
    public class CreateHintsHelper : ICreateHintsHelper
    {
        readonly IArchipelagoSocketHelper socket;
        readonly ILocationCheckHelper locationCheckHelper;
        readonly IRoomStateHelper roomStateHelper;
        readonly IPlayerHelper players;

        ///<inheritdoc/>
        public Version Version { get; private set; }
        
        internal CreateHintsHelper(IArchipelagoSocketHelper socket, IPlayerHelper players, ILocationCheckHelper locationCheckHelper, IRoomStateHelper roomStateHelper)
        {
            this.socket = socket;
            this.players = players;
            this.locationCheckHelper = locationCheckHelper;
            this.roomStateHelper = roomStateHelper;
        }
        /// <inheritdoc/>
        public void CreateHints(int player, HintStatus hintStatus = HintStatus.Unspecified, params long[] ids)
        {
            // The server supports CreateHints after version 0.6.2
            if (roomStateHelper.Version.CompareTo(new Version(0, 6, 2)) <= 0)
            {
                CreateHintsFallback(ids);
            }
            else
            {
                socket.SendPacket(new CreateHintsPacket
                {
                    Locations = ids,
                    Player = player,
                    Status = (int)hintStatus
                });
            }
        }

#if NET35

        /// <inheritdoc/>
        public void CreateHintsFallback(params long[] ids) => locationCheckHelper.ScoutLocationsAsync(null, true, ids);
#else
        /// <inheritdoc/>
        public void CreateHintsFallback(params long[] ids) => locationCheckHelper.ScoutLocationsAsync(true, ids);
#endif

        /// <inheritdoc/>
        public void CreateHints(HintStatus hintStatus = HintStatus.Unspecified, params long[] ids)
        {
            // When the player is not included, it defaults to the requesting slot.
            var currentPlayer = players.ActivePlayer.Slot;
            CreateHints(currentPlayer, hintStatus, ids);
        }
    }
}
