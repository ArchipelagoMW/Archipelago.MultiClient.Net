# Packets

@"Archipelago.MultiClient.Net.Packets?text=Packets" are the payload used for communicating with the Archipelago server.
For more fine-tuned handling, the library splits them up even more for digesting into the various helpers. This
document will only cover a few of the main ones and some methods for interacting with them.

### Manual Packet Handling

If you have a use case for a packet that isn't exposed, or can be handled through one of the available helpers, you can
manually subscribe to the socket's
@"Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates.PacketReceivedHandler?text=PacketReceivedHandler"
and process the packet from there.

```csharp
public static void RegisterBounceHandler()
{
    Session.Socket.PacketReceived += OnPacketReceived;
}

public static void OnPacketReceived(ArchipelagoPacketBase packet)
{
    switch (packet)
    {
        case ItemCheatPrintJsonPacket:
            Console.WriteLine("You're a dirty cheater.");
            break;
    }
}
```

## ReceivedItemsPacket

A @"Archipelago.MultiClient.Net.Packets.ReceivedItemsPacket?text=ReceivedItemsPacket" is received whenever the player
receives one or more items from the server. The @"Archipelago.MultiClient.Net.Helpers.IReceivedItemsHelper?text=Items Helper"
will digest the packet for you and should be used for any item processing.

```csharp
public static void RegisterItemsHandler()
{
    // subscribe to the ItemReceived event
    Session.Items.ItemReceived += HandleItem;
}

public static void HandleItem(ReceivedItemsHelper itemHandler)
{
    // The event is fired for every received item. Keep a local index to compare against in the case of continuing
    // an existing session after an earlier disconnection.
    var itemToHandle = itemHandler.DequeueItem();
    // Since we dequeued the current item already, the index on the handler is the index of the *next* upcoming item,
    // not the currently processing one.
    if (itemHandler.Index <= MyIndex)
    {
        return;
    }
    MyIndex++;
    // Do whatever other item handling
}
```

## LocationChecksPacket

@"Archipelago.MultiClient.Net.Packets.LocationChecksPacket?text=This Packet" is used for notifying the server of what
locations have been completed by the player. This can be achieved by using one of the
@"Archipelago.MultiClient.Net.Helpers.ILocationCheckHelper?text=Location Check Helper's" methods;
`CompleteLocationChecks` or `CompleteLocationChecksAsync`.

## LocationScoutsPacket

A @"Archipelago.MultiClient.Net.Packets.LocationScoutsPacket?text=LocationScoutsPacket" is sent to the server in order
to request locations get scouted. This can also be used to create hints for these locations. After this packet is sent,
the server will reply with a @"Archipelago.MultiClient.Net.Packets.LocationInfoPacket?text=LocationInfoPacket", which
is digested and returned to the callback as a dictionary of the location id to its
@"Archipelago.MultiClient.Net.Models.ScoutedItemInfo?text=ScoutedItemInfo".

```csharp
public static void ScoutLocations()
{
    // Send a LocationScoutsPacket to the server for locations 1, 2, and 3, register `HandleScoutInfo` as the
    // callback for the reply packet, and do not create any hints
    Session.Locations.ScoutLocationsAsync(HandleScouteInfo, new []{1, 2, 3});
}

public static void HandleScoutInfo(Dictionary<long, ScoutedItemInfo> scoutedLocationInfo)
{
    // handle the Info however necessary
}
```

## StatusUpdatePacket

The @"Archipelago.MultiClient.Net.Packets.StatusUpdatePacket?text=StatusUpdatePacket" is used to inform the server that
the player has changed their @"Archipelago.MultiClient.Net.Enums.ArchipelagoClientState?text=client status". The most
common usage of this is to tell the server the player is in game and has started playing, or that the player has
completed their goal. This packet type has helpers directly on the ArchipelagoSession with `SetClientState` and
`SetGoalAchieved`.

```csharp
public static void UpdateStatus(ArchipelagoClientState state)
{
    Session.SetClientState(state);
}
```

or

```csharp
public static void OnGoalCompleted()
{
    Session.SetGoalAchieved();
}
```

## SayPacket

A @"Archipelago.MultiClient.Net.Packets.SayPacket?text=SayPacket" is used to communicate with other players. This is also the packet to use if you are trying to send a client command to the server such as `!release`.

```csharp
public static void SendReleaseCommand()
{
    SendSayPacket("!release");
}

public static void SendSayPacket(string text)
{
    Session.Socket.SendPacket(new SayPacket { Text = text });
}
```

## BouncePacket

A @"Archipelago.MultiClient.Net.Packets.BouncePacket?text=BouncePacket" is a special type of packet that can be sent
and received to specific slots, games, or client tags. DeathLink is the most common usage of this and has its own
helper, but for any other use they will need to be handled manually.

```csharp
public static void ProcessBouncePacket(BouncePacket bouncePacket)
{
    foreach (var key in bouncePacket.Data.keys())
    {
        Console.WriteLine(key);
    }
}
```
