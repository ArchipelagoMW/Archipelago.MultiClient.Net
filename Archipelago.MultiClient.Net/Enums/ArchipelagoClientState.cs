namespace Archipelago.MultiClient.Net.Enums
{
    public enum ArchipelagoClientState : int
    {
        ClientUnknown = 0,
		ClientConnected = 5,
        ClientReady = 10,
        ClientPlaying = 20,
        ClientGoal = 30
    }
}
