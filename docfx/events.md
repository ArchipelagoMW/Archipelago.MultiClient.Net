
# Event Hooks

If using .net 4.0 or higher, you can use `ConnectAsync` and `LoginAsync` to prevent hitching for injection-based implementations like harmony.

## Message Logging

The Archipelago server can send messages to client to be displayed on screen as sort of a log, this is done by handling the `PrintJsonPacket` packets. This library simplifies this process into a single handler for you to handle.
```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);
session.MessageLog.OnMessageReceived += OnMessageReceived;
session.TryConnectAndLogin("Timespinner", "Jarno", ItemsHandlingFlags.AllItems, new Version(0,3,5));

static void OnMessageReceived(LogMessage message)
{
    DisplayOnScreen(message.ToString());
}
```

In some cased you might want extra information that is provided by the server in such cases you can use type checking

```csharp
static void OnMessageReceived(LogMessage message)
{
    switch (message)
    {
        case ItemHintLogMessage hintLogMessage:
            var receiver = itemSendLogMessage.Receiver;
            var sender = itemSendLogMessage.Sender;
            var networkItem = itemSendLogMessage.Item;
            var found = hintLogMessage.IsFound;
            break;
        case ItemSendLogMessage itemSendLogMessage: 
            var receiver = itemSendLogMessage.Receiver;
            var sender = itemSendLogMessage.Sender;
            var networkItem = itemSendLogMessage.Item;
            break;
    }

    DisplayOnScreen(message.ToString());
}
```

If you want more control over how the message is displayed, like for example you might want to color certain parts of the message,
Then you can use the `Parts` property. This returns each part of the message in order of appearnce with the `Text` to be displayed and also the `Color` it would normally be diplayed in.
If `IsBackgroundColor` is true, then the color should be applied to the message background instead.
The MessagePart message can also contain additional information that can be retrieved by type checking.


```csharp
foreach (part in message.Parts)
{
    switch (part)
    {
        case ItemMessagePart itemMessagePart: 
            var itemId = itemMessagePart.ItemId;
            var flags = itemMessagePart.Flags;
            break;
        case LocationMessagePart locationMessagePart:
            var locationId = locationMessagePart.LocationId;
            break;
        case PlayerMessagePart playerMessagePart:
            var slotId = playerMessagePart.SlotId;
            var isCurrentPlayer = playerMessagePart.IsActivePlayer;
            break;
    }

    DisplayOnScreen(part.Text, part.Color, part.IsBackgroundColor);
}
```
