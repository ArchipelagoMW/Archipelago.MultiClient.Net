using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;

#if NET35
#else
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
    /// <summary>
    /// Provides information about the current state of the server
    /// </summary>
    public interface IHintsHelper
    {
		/// <summary>
		///     Tell the server to create hints for the specified locations.
		///     When creating hints for another slot's locations, the packet will fail if any of those locations do not contain an item for the requesting slot.
		///     When creating hints for your own slot's locations, non-existing locations will be silently skipped.
		///
		///     NOTE: AP does not provide a way for you to know what locations exists in other players worlds, such information needs to be saved during generation if needed
		/// </summary>
		/// <param name="player">
		///     The ID of the player whose locations are being hinted for.
		/// </param>
		/// <param name="hintStatus">
		///     If included, sets the status of the created hints to this status.
		///     Defaults to unspecified.
		/// </param>
		/// <param name="locationIds">
		///     The location ids to create hints for.
		/// </param>
		/// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
		///     The websocket connection is not alive.
		/// </exception>
		void CreateHints(int player, HintStatus hintStatus = HintStatus.Unspecified, params long[] locationIds);

		/// <summary>
		///     Tell the server to create hints for the specified locations.
		///     This version does not include the player id, and as such will only hint for locations in the active player's game.
		///     When creating hints, non-existing locations will be silently skipped.
		///
		///    NOTE: AP does not provide a way for you to know what locations exists in other players worlds, such information needs to be saved during generation if needed
		/// </summary>
		/// <param name="hintStatus">
		///     If included, sets the status of the created hints to this status.
		///     Defaults to unspecified.
		/// </param>
		/// <param name="locationIds">
		///     The location ids to create hints for.
		/// </param>
		/// <exception cref="T:Archipelago.MultiClient.Net.Exceptions.ArchipelagoSocketClosedException">
		///     The websocket connection is not alive.
		/// </exception>
		void CreateHints(HintStatus hintStatus = HintStatus.Unspecified, params long[] locationIds);

		/// <summary>
		///    Update the hint status for a specific location in a specific players their world.
		///
		///    NOTE: AP does not provide a way for you to know what locations exists in other players worlds, such information needs to be saved during generation if needed
		/// </summary>
		/// <param name="player">The ID of the player whose location's hint is being updated.</param>
		/// <param name="locationId">The ID of the location in the players world to update</param>
		/// <param name="newHintStatus">The new desired hint status for this hint, cannot be status HintStatus.Found as that is automaticly handled by AP</param>
		void UpdateHintStatus(int player, long locationId, HintStatus newHintStatus);

		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		Hint[] GetHints(int? slot = null, int? team = null);

#if NET35
		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="onHintsRetrieved">the method to call with the retrieved hints</param>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		void GetHintsAsync(Action<Hint[]> onHintsRetrieved, int? slot = null, int? team = null);
#else
		/// <summary>
		/// Retrieves all unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		/// <returns>An array of unlocked hints or null if the slot or team does not exist</returns>
		Task<Hint[]> GetHintsAsync(int? slot = null, int? team = null);
#endif

		/// <summary>
		/// Sets a callback to be called with all updates to the unlocked hints for the specified player slot and team
		/// </summary>
		/// <param name="onHintsUpdated">the method to call with the updated hints</param>
		/// <param name="retrieveCurrentlyUnlockedHints">should the currently unlocked hints be retrieved or just the updates</param>
		/// <param name="slot">the slot id of the player to request hints for, defaults to the current player's slot if left empty</param>
		/// <param name="team">the team id of the player to request hints for, defaults to the current player's team if left empty</param>
		void TrackHints(Action<Hint[]> onHintsUpdated,
			bool retrieveCurrentlyUnlockedHints = true, int? slot = null, int? team = null);
	}

    ///<inheritdoc/>
    public class HintsHelper : IHintsHelper
    {
        readonly IArchipelagoSocketHelper socket;
        readonly ILocationCheckHelper locationCheckHelper;
        readonly IRoomStateHelper roomStateHelper;
        readonly IPlayerHelper players;
		readonly IDataStorageHelper dataStorageHelper;

        internal HintsHelper(IArchipelagoSocketHelper socket, IPlayerHelper players, ILocationCheckHelper locationCheckHelper, 
	        IRoomStateHelper roomStateHelper, IDataStorageHelper dataStorageHelper)
        {
            this.socket = socket;
            this.players = players;
            this.locationCheckHelper = locationCheckHelper;
            this.roomStateHelper = roomStateHelper;
			this.dataStorageHelper = dataStorageHelper;
        }

        /// <inheritdoc/>
        public void CreateHints(int player, HintStatus hintStatus = HintStatus.Unspecified, params long[] locationIds)
        {
            // The server supports CreateHints after version 0.6.2
            if (roomStateHelper.Version < new Version(0, 6, 2))
            {
#if NET35
				locationCheckHelper.ScoutLocationsAsync(null, createAsHint: true, locationIds);
#else
	            locationCheckHelper.ScoutLocationsAsync(createAsHint: true, locationIds);
#endif
            }
			else
            {
                socket.SendPacket(new CreateHintsPacket
                {
                    Locations = locationIds,
                    Player = player,
                    Status = hintStatus
                });
            }
        }

		/// <inheritdoc/>
        public void CreateHints(HintStatus hintStatus = HintStatus.Unspecified, params long[] ids)
        {
            // When the player is not included, it defaults to the requesting slot.
            var currentPlayer = players.ActivePlayer.Slot;
            CreateHints(currentPlayer, hintStatus, ids);
        }

		/// <inheritdoc/>
        public void UpdateHintStatus(int player, long locationId, HintStatus newHintStatus) =>
	        socket.SendPacket(new UpdateHintPacket
	        {
		        Location = locationId,
		        Player = player,
		        Status = newHintStatus
			});

        /// <inheritdoc/>
		public Hint[] GetHints(int? slot = null, int? team = null) => dataStorageHelper.GetHints(slot, team);


#if NET35
	    /// <inheritdoc/>
		public void GetHintsAsync(Action<Hint[]> onHintsRetrieved, int? slot = null, int? team = null) => 
			dataStorageHelper.GetHintsAsync(onHintsRetrieved, slot, team);
#else
	    /// <inheritdoc/>
		public Task<Hint[]> GetHintsAsync(int? slot = null, int? team = null) => 
		    dataStorageHelper.GetHintsAsync(slot, team);
#endif
		
		/// <inheritdoc/>
		public void TrackHints(Action<Hint[]> onHintsUpdated, bool retrieveCurrentlyUnlockedHints = true, int? slot = null, int? team = null) =>
			dataStorageHelper.TrackHints(onHintsUpdated, retrieveCurrentlyUnlockedHints, slot, team);
}
}
