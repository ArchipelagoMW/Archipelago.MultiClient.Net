# Archipelago.MultiClient.Net
A client library for use with .NET based prog-langs for interfacing with Archipelago hosts.

## Getting Started

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.Socket.Connect();
session.Locations.CompleteLocationChecks(42);
```

The `ArchipelagoSession` object is your entrypoint for all operations with the server. The session object exposes various helpers which provide functionality for various objectives you might be trying to achieve.

### LocationCheckHelper

The location check helper may be used to complete or scout location checks. The API also provides utility functions such as retrieving the name of a location from numerical id.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.Socket.Connect();
session.Locations.CompleteLocationChecks(42);

string locationName = session.Locations.GetLocationNameFromId(42);
int locationId = session.Locations.GetLocationIdFromName(locationName);

session.Locations.ScoutLocationsAsync(locationInfoPacket => Console.WriteLine(locationInfoPacket.Locations.Count));
```

### ReceivedItemsHelper

The received items helper provides an interface for retrieving the list of received items as well as a C# event for when an item is received. The event automatically executes its registered event handlers when an item is received. A utility method is also included for getting an item name from numerical id.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.Socket.Connect();
session.Items.ItemReceived += (receivedItemsHelper) => {
	var itemReceivedName = receivedItemsHelper.PeekItemName();
	// ... Handle item receipt.

	receivedItemsHelper.DequeueItem();
};
```

### PlayerHelper

The player helper provides methods for accessing details about the other players currently connected to the Archipelago session.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.Socket.Connect();

var sortedPlayerNames = session.Players.AllPlayers.Select(x => x.Name).OrderBy(x => x);
```

### ArchipelagoSocketHelper

The socket helper is a lower level API allowing for direct access to the socket which the session object uses to communicate with the Archipelago server. You may use this object to hook onto when messages are received or you may use it to send any packets defined in the library. Various events are exposed to allow for receipt of errors or notifying of socket close.

W

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.Socket.Connect();
session.Socket.SendPacket(new PrintPacket(){Text = "Woof woof!"});

```

## DeathLink
DeathLink support is included in the library. You may enable it by using the `CreateDeathLinkServiceAndEnable` in the `DeathLinkProvider` class. This method is also an extension method so you may enable it more easily. DeathLink should be enabled before the socket connection is opened.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

var deathLinkService = session.CreateDeathLinkServiceAndEnable();
session.Socket.Connect();

deathLinkService.OnDeathLinkReceived += (deathLinkObject) => {
	// ... Kill your player(s).
};

// ... On death:
deathLinkService.SendDeathLink(new DeathLink(){
	sourcePlayer = "Ijwu",
	cause = "Died to exposure."
});
```