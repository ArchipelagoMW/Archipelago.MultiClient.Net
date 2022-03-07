# Archipelago.MultiClient.Net
A client library for use with .NET based prog-langs for interfacing with Archipelago hosts.

## Getting Started

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));
session.Locations.CompleteLocationChecks(42);
```

The `ArchipelagoSession` object is your entrypoint for all operations with the server. The session object exposes various helpers which provide functionality for various objectives you might be trying to achieve.

### LocationCheckHelper

The location check helper may be used to complete or scout location checks. The API also provides utility functions such as retrieving the name of a location from numerical id.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));
session.Locations.CompleteLocationChecks(42);

string locationName = session.Locations.GetLocationNameFromId(42);
int locationId = session.Locations.GetLocationIdFromName(locationName);

session.Locations.ScoutLocationsAsync(locationInfoPacket => Console.WriteLine(locationInfoPacket.Locations.Count));
```

### ReceivedItemsHelper

The received items helper provides an interface for retrieving the list of received items as well as a C# event for when an item is received. The event automatically executes its registered event handlers when an item is received. A utility method is also included for getting an item name from numerical id.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));
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

session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));
var sortedPlayerNames = session.Players.AllPlayers.Select(x => x.Name).OrderBy(x => x);
```

### ArchipelagoSocketHelper

The socket helper is a lower level API allowing for direct access to the socket which the session object uses to communicate with the Archipelago server. You may use this object to hook onto when messages are received or you may use it to send any packets defined in the library. Various events are exposed to allow for receipt of errors or notifying of socket close.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));
session.Socket.SendPacket(new SayPacket(){Text = "Woof woof!"});

```

## DeathLink

DeathLink support is included in the library. You may enable it by using the `CreateDeathLinkServiceAndEnable` in the `DeathLinkProvider` class. This method is also an extension method so you may enable it more easily.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

var deathLinkService = session.CreateDeathLinkServiceAndEnable();
session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));

deathLinkService.OnDeathLinkReceived += (deathLinkObject) => {
	// ... Kill your player(s).
};

// ... On death:
deathLinkService.SendDeathLink(new DeathLink("Ijwu", "Died to exposure."));
```

## DataStorage

DataStorage support is included in the library. you may save values on the archipelago server in order to share there across other players or to simply keep track of values outside of your game's state

The DataStorage provides an interface based on keys and thier scope. by assinging a value to a key, that value is stored on the server, and by reading from a key a value is retrieved from the server. 
The DataStorage also provides methods to retrieve the value of key asynchronously. 
If your intrested in keeping track of when a value of a certian key is changed you can use the `OnValueChanged` handler to register a callback for when the value gets updated.
An `Initialize` Method is provided to set the initial value of a key without overriding any existing value.
Complex objects need to be stored in the form of a `JObject`, therefor you must wrap them into a `JObject.FromObject()`

Mathematical operations are supported using the following operators:
* `+`, Add right value to left value
* `-`, Subtract right value from left value
* `*`, Multiply right value by left value
* `/`, Divide right value by left value
* `%`, Gets remainder after dividing left value by right value
* `^`, Multiply left value by the power of the right value
* `>>`, Override left with right value. if right value is lower
* `<<`, Override left with right value. if right value is bigger

Bitwise operations are supported using the following opperations:
* `+ Bitwise.Xor(x)`, apply logical exclusive OR to the right value using value x
* `+ Bitwise.Or(x)`, apply logical OR to the right value using value x
* `+ Bitwise.And(x)`, apply logical AND to the right value using value x
* `+ Bitwise.LeftShift(x)`, binairy shift left to the left value by x
* `+ Bitwise.RightShift(x)`, binairy shift left to the right value by x

Operation specific callbacks are supported, these get called only once with the results of the current operation:
* '+ Callback.Add((oldValue, newValue) => {});', calls this method after your operation or chain of operations are proccesed

mathematical and bitwise operations can be chained, given the extended syntax with `()` around each operation

examples:
```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);
session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));

//Initializing
session.DataStorage.Initialize("B", 20); //Set initial value for B in global scope if it has no value assigned yet

//Storing/Updating
session.DataStorage[Scope.Slot, "A"] = 20; //Set A to 20, in scope of the current connected user\slot
session.DataStorage[Scope.Global, "A"] = 30; //Set A to 30, in gloval scope shared amoung all players (the default scope is global)
session.DataStorage["B"] += 50; //Add 50 to the current value of B
session.DataStorage["C"] /= 2; //Divide current value of C in half
session.DataStorage["D"] <<= 80; //Set D to 80 if the stored value is lower
session.DataStorage["F"] = JObject.FromObject(new { Number = 10, Text = "Hello" }); //Set F to a custom object
session.DataStorage["C"] += Bitwise.LeftShift(1); //Divide current value of C in half, again
session.DataStorage["G"] += Bitwise.Xor(0xFF); //Moddify G using the Bitwise excluse or operation
session.DataStorage["H"] = session.DataStorage["I"] - 30; //Get value of I, Assign it to H and than subtract 30
session.DataStorage["J"] = new []{ "One", "Two" }; //Arrays can be stored directly, List's needs to be converted ToArray() first 
session.DataStorage["J"] += new []{ "Three" }; //Append array to existing array on the server

//Chaining operations
session.DataStorage["K"] = (session.DataStorage["K"] + 40) >> 100; //Add 40 to G, than Set G to 100 if G is bigger then 100
session.DataStorage["L"] = ((session.DataStorage["M"] - 6) + Bitwise.RightShift(1)) ^ 3; //Subtract 6 from I, than multiply I by 2 using bitshifting, than take I to the power of 3

//Update callbacks
//Enerylink deplete pattern, Subtract 50, than set value to 0 if its lower than 0
session.DataStorage["EnergyLink"] = ((session.DataStorage["EnergyLink"] - 50) << 0) + Callback.Add((value, newValue) =>
{
    var actualDepletedValue = (float)newValue - (float)value; //calculate the actual update, might differ if there was less that 50 left on the server
});

//Keepking track of changes
session.DataStorage["N"].OnValueChaned += (old, new) => {
	var changed = (int)new - (int)old; //Keep track of changes made to E by anyone client, and calculate the difference
};

//Retrieving
session.DataStorage.GetAsync<string>("O", s => { string r = s }); //Retrieve value of M asynchronously
float c = session.DataStorage["C"]; //Retrieve value for C
var d = session.DataStorage["T"].To<DateTime>() //Retrieve value for T as a DateTime struct
var array = session.DataStorage["J"].To<string[]>() //Retrieve value for J as string Array
var obj = session.DataStorage["F"].To<JObject>(); //Retrieve value for F where an anonymous object was stored
var value = (int)obj["Number"]; //Get value for anonymous object key A
var text = (string)obj["Text"]; //Get value for anonymous object key B

```