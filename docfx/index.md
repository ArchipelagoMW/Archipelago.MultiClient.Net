# Introduction

Archipelago.MultiClient.net is a client library for use with .NET based applications for interfacing with
[Archipelago](https://github.com/ArchipelagoMW/Archipelago) server hosts. This library conforms with the latest stable
[Archipelago Network Protocol Specification](https://github.com/ArchipelagoMW/Archipelago/blob/main/docs/network%20protocol.md).

## Installing the package

First, install the [Archipelago.MultiClient.Net package from NuGet](https://www.nuget.org/packages/Archipelago.MultiClient.Net).
NuGet provides several options for how to do this with examples given on the page. Optionally, you can also install the
community-supported [Archipelago.MultiClient.Net.Analyzers package](https://www.nuget.org/packages/Archipelago.MultiClient.Net.Analyzers),
which provides source generators, code analysis, and fixes to help prevent and correct common usage errors at compile
time.

## Concepts

### Session

The @"Archipelago.MultiClient.Net.ArchipelagoSession?text=ArchipelagoSession" is the basis of all operations involved
with communicating with the Archipelago server. Before any communicating can be executed, a new session must be created,
and any relevant [event hooks](docs/events.md) should be registered before attempting a connection.

### Helpers

There exist many [helper classes and methods](docs/helpers.md) to help digest the information received from the server
into specific formats, or to send specific information to the server. If a helper doesn't do what you need, the raw
server data is also available to do what is needed through the
@"Archipelago.MultiClient.Net.Helpers.IArchipelagoSocketHelper?text=Session.Socket".

### Packets

Communication with the Archipelago server is done by sending and receiving various json packets through the websocket
connection. Most packets that you want to send will have abstraction layers available through the helpers, but sometimes
there is need to send raw packets such as a @"Archipelago.MultiClient.Net.Packets.SayPacket?text=SayPacket".
