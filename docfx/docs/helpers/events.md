# Event Hooks

## ArchipelagoSocket

@"Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelper?text=ArchipelagoSocketHelper", accessible through
`Session.Socket`

| Event                                                                                                             | Call Event                                                                        |
|-------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------|
| @"Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates.ErrorReceivedHandler?text=ErrorReceived"   | Called when there is an error in the socket connection or while parsing a packet. |
| @"Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates.PacketReceivedHandler?text=PacketReceived" | Called when a packet has been received from the server and identified.            |
| @"Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates.PacketsSentHandler?text=PacketsSent"       | Called just before submitting a packet to the server.                             |
| @"Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates.SocketClosedHandler?text=SocketClosed"     | Called when the underlying socket connection has been closed.                     |
| @"Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates.SocketOpenedHandler?text=SocketOpened"     | Called when the underlying socket connection is opened to the server.             |

## ReceivedItemsHelper

@"Archipelago.MultiClient.Net.Helpers.IReceivedItemsHelper?text=ReceivedItemsHelper", accessible through `Session.Items`.

| Event                                                                                            | Call Event                                                                                                                |
|--------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------|
| @"Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper.ItemReceivedHandler?text=ItemReceived" | When an item is received. If multiple items are received in a single packet, the event is fired for each individual item. |

## LocationCheckHelper

@"Archipelago.MultiClient.Net.Helpers.LocationCheckHelper?text=LocationCheckHelper", accessible through
`Session.Locations`.

| Event                                                                                                                  | Call Event                                                                   |
|------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------|
| @"Archipelago.MultiClient.Net.Helpers.LocationCheckHelper.CheckedLocationsUpdatedHandler?text=CheckedLocationsUpdated" | Called when new locations are checked, such as another player using !collect |

## MessageLogHelper

@"Archipelago.MultiClient.Net.Helpers.MessageLogHelper?text=MessageLogHelper", accessible through `Session.MessageLog`

| Event                                                                                                 | Call Event                                                       |
|-------------------------------------------------------------------------------------------------------|------------------------------------------------------------------|
| @"Archipelago.MultiClient.Net.Helpers.MessageLogHelper.MessageReceivedHandler?text=OnMessageReceived" | Called for each message that should be displayed for the player. |

### Message Logging

The Archipelago server can send messages to client to be displayed on screen as a sort of log, this is done by handling
the `PrintJsonPacket` packets. This library simplifies this process into a
@"Archipelago.MultiClient.Net.Helpers.IMessageLogHelper?text='single handler'".

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

If you want more control over how the message is displayed, like for example you might want to color certain parts of
the message, then you can use the `Parts` property. This returns each part of the message in order of appearance with
the `Text` to be displayed and also the `Color` it would normally be displayed in. If `IsBackgroundColor` is true, then
the color should be applied to the message background instead. The MessagePart message can also contain additional
information that can be retrieved by type checking.

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
