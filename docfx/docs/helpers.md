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

The player helper provides methods for accessing details about the other players currently connected to the Archipelago
session.

### Get All Player Names

```csharp
var sortedPlayerNames = session.Players.AllPlayers.Select(x => x.Name).OrderBy(x => x);
```

### Get Current Player Name

```csharp
string playerName = session.Players.GetPlayerAliasAndName(session.ConnectionInfo.Slot);
```

## Locations

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

### Location ID <--> Name

```csharp
string locationName = session.Locations.GetLocationNameFromId(42) ?? $"Location: {locationId}";
long locationId = session.Locations.GetLocationIdFromName(locationName);
```

### Scout Location Checks

Scouting means asking the server what is stored in a specific location *without* collecting it:

```csharp
session.Locations.ScoutLocationsAsync(locationInfoPacket => Console.WriteLine(locationInfoPacket.Locations.Count));
```

## Items

### Item ID --> Name

```csharp
string itemName = session.Items.GetItemName(88) ?? $"Item: {itemId}";
```

### Access Received Items

At any time, you can access the current inventory for the active session/slot via the `Items` helper like so:

```csharp
foreach(NetworkItem item in session.Items.AllItemsReceived)
{
    long itemId = item.Item;
}
```

*Note: The list of received items will never shrink and the collection is guaranteed to be in the order that the server
sent the items. Because of this, it is safe to assume that if the size of this collection has changed it's because new
items were received and appended to the end of the collection.*

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

The RoomState helper provides access to values that represent the current state of the multiworld room, with information
such as the cost of a hint and or your current accumulated amount of hint point or the permissions for things like
forfeiting

```csharp
Console.WriteLine($"You have {session.RoomState.HintPoints}, and need {session.RoomState.HintCost} for a hint");
```

## ConnectionInfo

The ConnectionInfo helper provides access to values under which you are currently connected, such as your slot number or
your currently used tags and item handling flags

```csharp
Console.WriteLine($"You are connected on slot {session.ConnectionInfo.Slot}, on team {session.ConnectionInfo.Team}");
```

## ArchipelagoSocket

The socket helper is a lower level API allowing for direct access to the socket which the session object uses to
communicate with the Archipelago server. You may use this object to hook onto when messages are received, or you may use
it to send any packets defined in the library. Various events are exposed to allow for receipt of errors or notifying of
socket close.

```csharp
session.Socket.SendPacket(new SayPacket(){Text = "Woof woof!"});
```

## DeathLink

DeathLink support is included in the library. You may enable it by using the `CreateDeathLinkService` in the
`DeathLinkProvider` class, and the `EnableDeathlink` method on the service. Deathlink can be toggled on and off using
the `EnableDeathlink` and `DisableDeathlink` methods on the service.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

var deathLinkService = session.CreateDeathLinkService().EnableDeathlink();

deathLinkService.OnDeathLinkReceived += (deathLinkObject) => {
    // ... Kill your player(s).
};

// ... On death:
deathLinkService.SendDeathLink(new DeathLink("Ijwu", "Died to exposure."));
```
