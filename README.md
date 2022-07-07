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

string locationName = session.Locations.GetLocationNameFromId(42) ?? $"Location: {locationId}";
int locationId = session.Locations.GetLocationIdFromName(locationName);

session.Locations.ScoutLocationsAsync(locationInfoPacket => Console.WriteLine(locationInfoPacket.Locations.Count));
```

### ReceivedItemsHelper

The received items helper provides an interface for retrieving the list of received items as well as a C# event for when an item is received. The event automatically executes its registered event handlers when an item is received. A utility method is also included for getting an item name from numerical id.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));
session.Items.ItemReceived += (receivedItemsHelper) => {
	var itemReceivedName = receivedItemsHelper.PeekItemName() ?? $"Item: {itemId}";
	// ... Handle item receipt.

	receivedItemsHelper.DequeueItem();
};

string itemName = session.Items.GetItemName(88) ?? $"Item: {itemId}"
```

### PlayerHelper

The player helper provides methods for accessing details about the other players currently connected to the Archipelago session.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));
var sortedPlayerNames = session.Players.AllPlayers.Select(x => x.Name).OrderBy(x => x);
```

### RoomStateHelper

The roomstate helper provides access to values that represent the current state of the multiworld room, with information such as the cost of a hint and or your current accumulated amount of hint point or the permissions for things like forfeiting

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);
session.TryConnectAndLogin("Timespinner", "Jarno", new Version(2,1,0));

Console.WriteLine($"You have {session.RoomState.HintPoints}, and need {session.RoomState.HintCost} for a hint");
```

### ConnectionInfoHelper

The conection info helper provides access to values under which you are currently connected, such as your slot number or your currently used tags and item handling flags

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);
session.TryConnectAndLogin("Timespinner", "Jarno", new Version(2,4,0));

Console.WriteLine($"You are connected on slot {session.ConnectionInfo.Slot}, on team {session.ConnectionInfo.Team}");
```

### ArchipelagoSocketHelper

The socket helper is a lower level API allowing for direct access to the socket which the session object uses to communicate with the Archipelago server. You may use this object to hook onto when messages are received or you may use it to send any packets defined in the library. Various events are exposed to allow for receipt of errors or notifying of socket close.

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", new Version(2,1,0));
session.Socket.SendPacket(new SayPacket(){Text = "Woof woof!"});

```

### DeathLink

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

### DataStorage

DataStorage support is included in the library. You may save values on the archipelago server in order to share them across other player's sessions or to simply keep track of values outside of your game's state.

The DataStorage provides an interface based on keys and their scope. By assigning a value to a key, that value is stored on the server and by reading from a key a value is retrieved from the server. 
Assigning and reading values from the store can be done using simple assignments `=`:
* `= session.DataStorage["Key"]`, read value from the data storage synchronously
* `session.DataStorage["Key"] =`, write value to the data storage asynchronously
  * Complex objects need to be stored and retrieved in the form of a `JObject`, therefor you must wrap them into a `JObject.FromObject()`

The DataStorage also provides methods to retrieve the value of a key asynchronously using `[key].GetAsync`. 
To set the initial value of a key without overriding any existing value the `[key].Initialize` method can be used.
If you're interested in keeping track of when a value of a certain key is changed by any client you can use the `[key].OnValueChanged` handler to register a callback for when the value gets updated.

Mathematical operations on values stored on the server are supported using the following operators:
* `+`, Add right value to left value
* `-`, Subtract right value from left value
* `*`, Multiply left value by right value
* `/`, Divide left value by right value
* `%`, Gets remainder after dividing left value by right value
* `^`, Multiply left value by the power of the right value
* `>>`, Override left with right value, if right value is lower
* `<<`, Override left with right value, if right value is bigger

Bitwise operations on values stored on the server are supported using the following opperations:
* `+ Bitwise.Xor(x)`, apply logical exclusive OR to the left value using value x
* `+ Bitwise.Or(x)`, apply logical OR to the left value using value x
* `+ Bitwise.And(x)`, apply logical AND to the left value using value x
* `+ Bitwise.LeftShift(x)`, binary shift the left value to the left by x
* `+ Bitwise.RightShift(x)`, binary shift the left value to the right by x

Operation specific callbacks are supported, these get called only once with the results of the current operation:
* `+ Callback.Add((oldValue, newValue) => {});`, calls this method after your operation or chain of operations are proccesed by the server

Mathematical operations, bitwise operations and callbacks can be chained, given the extended syntax with `()` around each operation.

Examples:
```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);
session.TryConnectAndLogin("Timespinner", "Jarno", new Version(2,6,0));

//Initializing
session.DataStorage["B"].Initialize(20); //Set initial value for B in global scope if it has no value assigned yet

//Storing/Updating
session.DataStorage[Scope.Slot, "SetPersonal"] = 20; //Set `SetPersonal` to 20, in scope of the current connected user\slot
session.DataStorage[Scope.Global, "SetGlobal"] = 30; //Set `SetGlobal` to 30, in global scope shared among all players (the default scope is global)
session.DataStorage["Add"] += 50; //Add 50 to the current value of `Add`
session.DataStorage["Divide"] /= 2; //Divide current value of `Divide` in half
session.DataStorage["Max"] <<= 80; //Set `Max` to 80 if the stored value is lower than 80
session.DataStorage["Dictionary"] = JObject.FromObject(new Dictionary<string, int>()); //Set `Dictionary` to a Dictionary
session.DataStorage["SetObject"] = JObject.FromObject(new SomeClassOrStruct()); //Set `SetObject` to a custom object
session.DataStorage["BitShiftLeft"] += Bitwise.LeftShift(1); //Bitshift current value of `BitShiftLeft` to left by 1
session.DataStorage["Xor"] += Bitwise.Xor(0xFF); //Modify `Xor` using the Bitwise exclusive or operation
session.DataStorage["DifferentKey"] = session.DataStorage["A"] - 30; //Get value of `A`, Assign it to `DifferentKey` and then subtract 30
session.DataStorage["Array"] = new []{ "One", "Two" }; //Arrays can be stored directly, List's needs to be converted ToArray() first 
session.DataStorage["Array"] += new []{ "Three" }; //Append array values to existing array on the server

//Chaining operations
session.DataStorage["Min"] = (session.DataStorage["Min"] + 40) >> 100; //Add 40 to `Min`, then Set `Min` to 100 if `Min` is bigger than 100
session.DataStorage["C"] = ((session.DataStorage["C"] - 6) + Bitwise.RightShift(1)) ^ 3; //Subtract 6 from `C`, then multiply `C` by 2 using bitshifting, then take `C` to the power of 3

//Update callbacks
//EnergyLink deplete pattern, subtract 50, then set value to 0 if its lower than 0
session.DataStorage["EnergyLink"] = ((session.DataStorage["EnergyLink"] - 50) << 0) + Callback.Add((old, new) => {
    var actualDepleted = (float)new - (float)old; //calculate the actual change, might differ if there was less than 50 left on the server
});

//Keeping track of changes
session.DataStorage["OnChangeHandler"].OnValueChanged += (old, new) => {
	var changed = (int)new - (int)old; //Keep track of changes made to `OnChangeHandler` by any client, and calculate the difference
};

//Retrieving
session.DataStorage["Async"].GetAsync<string>(s => { string r = s }); //Retrieve value of `Async` asynchronously
float f = session.DataStorage["Float"]; //Retrieve value for `Float` synchronously and store it as a float
var d = session.DataStorage["DateTime"].To<DateTime>() //Retrieve value for `DateTime` as a DateTime struct
var array = session.DataStorage["Strings"].To<string[]>() //Retrieve value for `Strings` as string Array

//Handling anonymous object, if the target type is not known you can use `To<JObject>()` and use its interface to access the members
session.DataStorage["Anonymous"] = JObject.FromObject(new { Number = 10, Text = "Hello" }); //Set `Anonymous` to an anonymous object
var obj = session.DataStorage["Anonymous"].To<JObject>(); //Retrieve value for `Anonymous` where an anonymous object was stored
var number = (int)obj["Number"]; //Get value for anonymous object key `Number`
var text = (string)obj["Text"]; //Get value for anonymous object key `Text`

```
