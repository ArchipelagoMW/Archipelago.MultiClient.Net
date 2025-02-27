# Getting Started

## Installing the package

First, install [the Archipelago.MultiClient.Net package from NuGet](https://www.nuget.org/packages/Archipelago.MultiClient.Net).
NuGet provides several options for how to do this with examples given on the page. Optionally, you can also install the
community-supported [Archipelago.MultiClient.Net.Analyzers package](https://www.nuget.org/packages/Archipelago.MultiClient.Net.Analyzers),
which provides source generators, code analysis, and fixes to help prevent and correct common usage errors at compile
time.

## Create Session Instance

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

// alternatively...
var session = ArchipelagoSessionFactory.CreateSession(new Uri("ws://localhost:38281"));
var session = ArchipelagoSessionFactory.CreateSession("localhost:38281");
var session = ArchipelagoSessionFactory.CreateSession("localhost");
```

The freshly created `ArchipelagoSession` object is the entrypoint for all incoming and outgoing interactions with the
server. Keep it in scope for at least the lifetime of the connection. If the room changes ports, or the user needs to
connect to a different room, then a new session needs to be created at the new host and port.

## Connect to Room

Connect to a server at a specific room slot using the following method:

```csharp
LoginResult TryConnectAndLogin(
        string game, // Name of the game implemented by this client, SHOULD match what is used in the world implementation
        string name, // Name of the slot to connect as (a.k.a player name)
        ItemsHandlingFlags itemsHandlingFlags, /* One of the following (see AP docs for details):
                NoItems
                RemoteItems
                IncludeOwnItems
                IncludeStartingInventory
                AllItems
            */
        Version version = null, // Minimum Archipelago API specification version which this client can successfuly interface with
        string[] tags = null, /* One of the following (see AP docs for details)
                "DeathLink"
                "Tracker"
                "TextOnly"
            */
        string uuid = null, // Unique identifier for this player/client, if null randomly generated
        string password = null, // Password that was set when the room was created
        bool requestSlotData = true // If the LoginResult should contain the slot data
    );
```

For example,

```csharp
LoginResult result = session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", ItemsHandlingFlags.AllItems);
```

would attempt to connect to a password-less room with the slot name `Ijwu`, report the game as `Risk of Rain 2`, and
tell the server that we need to be sent ReceivedItems Packets for all item sources.

Once connected, you have access to a suite of helper objects which provide an interface for sending/receiving
information with the server.

### Example Connection

```csharp
private static void Connect(string server, string user, string pass)
{
    LoginResult result;

    try
    {
        // handle TryConnectAndLogin attempt here and save the returned object to `result`
         result = session.TryConnectAndLogin("Risk of Rain 2", "Ijwu", ItemsHandlingFlags.AllItems);
    }
    catch (Exception e)
    {
        result = new LoginFailure(e.GetBaseException().Message);
    }

    if (!result.Successful)
    {
        LoginFailure failure = (LoginFailure)result;
        string errorMessage = $"Failed to Connect to {server} as {user}:";
        foreach (string error in failure.Errors)
        {
            errorMessage += $"\n    {error}";
        }
        foreach (ConnectionRefusedError error in failure.ErrorCodes)
        {
            errorMessage += $"\n    {error}";
        }

        return; // Did not connect, show the user the contents of `errorMessage`
    }
    
    // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be
    // used to interact with the server and the returned `LoginSuccessful` contains some useful information about the
    // initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
    var loginSuccess = (LoginSuccessful)result;
}
```
