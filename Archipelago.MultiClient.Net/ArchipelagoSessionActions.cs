using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;

namespace Archipelago.MultiClient.Net
{
	/// <summary>
	/// Actions that can be performed in the context of an session
	/// </summary>
	public interface IArchipelagoSessionActions
	{
		/// <summary>
		/// Send a message on behalf of the current player
		/// Can also be used to send commands like !hint item
		/// </summary>
		/// <param name="message">The message that will be broadcasted to the server and all clients</param>
		void Say(string message);

		/// <summary>
		/// Updates the status of the current player
		/// </summary>
		/// <param name="state">the state to send to the server</param>
		void SetClientState(ArchipelagoClientState state);

		/// <summary>
		/// Informs the server that the current player has reached their goal
		/// This cannot be reverted
		/// </summary>
		void SetGoalAchieved();
	}

	public partial class ArchipelagoSession : IArchipelagoSessionActions
	{
		/// <inheritdoc/>
		public void Say(string message) => Socket.SendPacket(new SayPacket { Text = message });

		/// <inheritdoc/>
		public void SetClientState(ArchipelagoClientState state) => Socket.SendPacket(new StatusUpdatePacket { Status = state });

		/// <inheritdoc/>
		public void SetGoalAchieved() => SetClientState(ArchipelagoClientState.ClientGoal);
	}
}
