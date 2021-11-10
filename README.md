# Archipelago.MultiClient.Net
A client library for use with .NET based prog-langs for interfacing with Archipelago hosts.

## Getting Started

```csharp
var hostUrl = "ws://localhost:38281";
var session = ArchipelagoSessionFactory.CreateSession(hostUrl);

session.Socket.Connect();
session.Locations.CompleteLocationChecks(42);
```
