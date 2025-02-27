
## Send Completion

You can report the completion of the player's goal like so:

```csharp
public static void send_completion()
{
    var statusUpdatePacket = new StatusUpdatePacket();
    statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
    Session.Socket.SendPacket(statusUpdatePacket);
}
```
