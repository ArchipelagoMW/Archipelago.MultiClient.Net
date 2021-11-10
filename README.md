# Archipelago.MultiClient.Net
A client library for use with .NET based prog-langs for interfacing with Archipelago hosts.

## Getting Started

```csharp
var session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

session.Socket.Connect();
session.Locations.CompleteLocationChecks(42);
```
