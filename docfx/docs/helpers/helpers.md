# Helper Overview

```csharp
session.Socket         // Payload-agnostic interface for sending/receving the most basic transmission units between client/server
session.Items          // Helpers for handling receipt of items
session.Locations      // Helpers for reporting visited locations to the server
session.Players        // Helpers for translating player information such as number, alias, name etc.
session.DataStorage    // Helpers for reading/writing data globally accessible to any client connected in the room
session.ConnectionInfo // Helpers for reading/handling useful information on the current connection
session.RoomState      // Information on the state of the room
session.MessageLog     // Interface for the server to push info messages to the user
```

## Players

The @"Archipelago.MultiClient.Net.Helpers.IPlayerHelper?text=Player Helper" provides methods for accessing details
about the other players currently connected to the Archipelago
session.

## Locations

The @"Archipelago.MultiClient.Net.Helpers.ILocationCheckHelper?text=Locations Helper" provides methods for accessing
information regarding the current player's locations, as well as updating the server on the status of their locations.

### Report Collected Location(s)

Call the following method to inform the server of locations whose items have been "found", and therefore should be
distributed if necessary:

```csharp
// Report multiple at once
session.Locations.CompleteLocationChecks(new []{1,3,8});

// Or report one at a time
session.Locations.CompleteLocationChecks(3);
```

The location ID used is of that defined in the AP world.

### Scout Location Checks

Scouting means asking the server what is stored in a specific location *without* collecting it. This can also be
utilized in order to create a hint, if - for instance - the current player knows what is on the location to inform other
players of this knowledge.

```csharp
session.Locations.ScoutLocationsAsync(locationInfoPacket => Console.WriteLine(locationInfoPacket.Locations.Count), HintCreationPolicy.CreateAndAnnounceOnce, new []{4, 5});
```

## Items

The @"Archipelago.MultiClient.Net.Helpers.IReceivedItemsHelper?text=Received Items Helper" provides methods for
checking the player's current inventory, and receiving items from the server.

### Received Item Callback Handler (Asynchronous)

```csharp
// Must go BEFORE a successful connection attempt
session.Items.ItemReceived += (receivedItemsHelper) => {
    var itemReceivedName = receivedItemsHelper.PeekItemName() ?? $"Item: {itemId}";

    // ... Handle item receipt here

    receivedItemsHelper.DequeueItem();
};
```

*Note: This callback event will occur for every item on connection and reconnection.*

## RoomState

The @"Archipelago.MultiClient.Net.Helpers.IRoomStateHelper?text=RoomState Helper" provides access to values that
represent the current state of the multiworld room, with information such as the cost of a hint and or your current
accumulated amount of hint point or the permissions for things like forfeiting.

```csharp
Console.WriteLine($"You have {session.RoomState.HintPoints}, and need {session.RoomState.HintCost} for a hint");
```

## ConnectionInfo

The @"Archipelago.MultiClient.Net.Helpers.IConnectionInfoProvider?text=ConnectionInfo Helper" provides access to
values under which you are currently connected, such as your slot number or your currently used tags and item handling
flags.

```csharp
Console.WriteLine($"You are connected on slot {session.ConnectionInfo.Slot}, on team {session.ConnectionInfo.Team}");
```

## ArchipelagoSocket

The @"Archipelago.MultiClient.Net.Helpers.IConnectionInfoProvider?text=Socket Helper" is a lower level API allowing
for direct access to the socket which the session object uses to communicate with the Archipelago server. You may use
this object to hook onto when messages are received, or you may use it to send any packets defined in the library.
Various events are exposed to allow for receipt of errors or notifying of socket close.

```csharp
session.Socket.SendPacket(new SayPacket(){Text = "Woof woof!"});
```

## MessageLog

The Archipelago server can send messages to client to be displayed on screen as a sort of log, this is done by handling
the `PrintJsonPacket` packets. This library simplifies this process into a
@"Archipelago.MultiClient.Net.Helpers.IMessageLogHelper?text=single handler" that can be subscribed to with an
[event hook](docs/helpers/events.md).

## DeathLink

DeathLink support is included in the library. You may enable it by creating a new
@"Archipelago.MultiClient.Net.BounceFeatures.DeathLink.DeathLinkService?text=DeathLinkService" from the
@"Archipelago.MultiClient.Net.BounceFeatures.DeathLink.DeathLinkProvider?text=DeathLinkProvider`, and subscribing to the
`OnDeathLinkReceived` event. Deathlink can then be toggled on and off using the `EnableDeathlink` and `DisableDeathlink`
methods on the service.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

var deathLinkService = session.CreateDeathLinkService();

deathLinkService.OnDeathLinkReceived += (deathLinkObject) => {
    // ... Kill your player(s).
};

deathLinkService.EnableDeathlink();
// ... On death:
deathLinkService.SendDeathLink(new DeathLink("Ijwu", "Died to exposure."));
```
