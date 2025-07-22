# DataStorage

DataStorage support is included in the library. You may save values on the archipelago server in order to share them
across other player's sessions or to simply keep track of values outside your game's state.

The DataStorage provides an interface based on keys and their scope. By assigning a value to a key, that value is stored
on the server and by reading from a key a value is retrieved from the server.
Assigning and reading values from the store can be done using simple assignments `=`:

* `= session.DataStorage["Key"]`, read value from the data storage synchronously
* `session.DataStorage["Key"] =`, write value to the data storage asynchronously
* Complex objects need to be stored and retrieved in the form of a `JObject`, therefore you must wrap them into a
  `JObject.FromObject()`

The DataStorage also provides methods to retrieve the value of a key asynchronously using `[key].GetAsync`.
To set the initial value of a key without overriding any existing value, the `[key].Initialize` method can be used.
If you're interested in keeping track of when a value of a certain key is changed by any client you can use the
`[key].OnValueChanged` handler to register a callback for when the value gets updated.

Mathematical operations on values stored on the server are supported using the following operators:

* `+`, Add right value to left value
* `-`, Subtract right value from left value
* `*`, Multiply left value by right value
* `/`, Divide left value by right value
* `%`, Gets remainder after dividing left value by right value
* `^`, Multiply left value by the power of the right value

Bitwise operations on values stored on the server are supported using the following operations:

* `+ Bitwise.Xor(x)`, apply logical exclusive OR to the left value using value x
* `+ Bitwise.Or(x)`, apply logical OR to the left value using value x
* `+ Bitwise.And(x)`, apply logical AND to the left value using value x
* `+ Bitwise.LeftShift(x)`, binary shift the left value to the left by x
* `+ Bitwise.RightShift(x)`, binary shift the left value to the right by x

Other operations on values stored on the server are supported using the following operations:

* `+ Operation.Min(x)`, get the lowest value of the left value and x
* `+ Operation.Max(x)`, get the highest value of the left value and x
* `+ Operation.Remove(x)`, when the left value is a list, removes the first element with value x
* `+ Operation.Pop(x)`, when the left value is a list or dictionary, removes the element at index x or key x
* `+ Operation.Update(x)`, when the left value is a list or dictionary, updates the dictionary with the keys/values in x

Operation specific callbacks are supported, these get called only once with the results of the current operation:

* `+ Callback.Add((oldValue, newValue) => {});`, calls this method after your operation or chain of operations are
  processed by the server

Mathematical operations, bitwise operations and callbacks can be chained, given the extended syntax with `()` around
each operation.

Examples:

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);
session.TryConnectAndLogin("Timespinner", "Jarno", ItemsHandlingFlags.AllItems);

//Initializing
session.DataStorage["B"].Initialize(20); //Set initial value for B in global scope if it has no value assigned yet

//Storing/Updating
session.DataStorage[Scope.Slot, "SetPersonal"] = 20; //Set `SetPersonal` to 20, in scope of the current connected user\slot
session.DataStorage[Scope.Global, "SetGlobal"] = 30; //Set `SetGlobal` to 30, in global scope shared among all players (the default scope is global)
session.DataStorage["Add"] += 50; //Add 50 to the current value of `Add`
session.DataStorage["Divide"] /= 2; //Divide current value of `Divide` in half
session.DataStorage["Max"] += + Operation.Max(80); //Set `Max` to 80 if the stored value is lower than 80
session.DataStorage["Dictionary"] = JObject.FromObject(new Dictionary<string, int>()); //Set `Dictionary` to a Dictionary
session.DataStorage["SetObject"] = JObject.FromObject(new SomeClassOrStruct()); //Set `SetObject` to a custom object
session.DataStorage["BitShiftLeft"] += Bitwise.LeftShift(1); //Bitshift current value of `BitShiftLeft` to left by 1
session.DataStorage["Xor"] += Bitwise.Xor(0xFF); //Modify `Xor` using the Bitwise exclusive or operation
session.DataStorage["DifferentKey"] = session.DataStorage["A"] - 30; //Get value of `A`, Assign it to `DifferentKey` and then subtract 30
session.DataStorage["Array"] = new []{ "One", "Two" }; //Arrays can be stored directly, List's needs to be converted ToArray() first 
session.DataStorage["Array"] += new []{ "Three" }; //Append array values to existing array on the server

//Chaining operations
session.DataStorage["Min"] = (session.DataStorage["Min"] + 40) + Operation.Min(100); //Add 40 to `Min`, then Set `Min` to 100 if `Min` is bigger than 100
session.DataStorage["C"] = ((session.DataStorage["C"] - 6) + Bitwise.RightShift(1)) ^ 3; //Subtract 6 from `C`, then multiply `C` by 2 using bitshifting, then take `C` to the power of 3

//Update callbacks
//EnergyLink deplete pattern, subtract 50, then set value to 0 if its lower than 0
session.DataStorage["EnergyLink"] = ((session.DataStorage["EnergyLink"] - 50) + Operation.Min(0)) + Callback.Add((oldData, newData) => {
    var actualDepleted = (float)newData - (float)oldData; //calculate the actual change, might differ if there was less than 50 left on the server
});

//Keeping track of changes
session.DataStorage["OnChangeHandler"].OnValueChanged += (oldData, newData) => {
    var changed = (int)newData - (int)oldData; //Keep track of changes made to `OnChangeHandler` by any client, and calculate the difference
};

//Keeping track of change (but for more complex data structures)
session.DataStorage["OnChangeHandler"].OnValueChanged += (oldData, newData) => {
    var old_dict = oldData.ToObject<Dictionary<int, int>>();
    var new_dict = newData.ToObject<Dictionary<int, int>>();
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
