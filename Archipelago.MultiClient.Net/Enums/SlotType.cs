using System;

namespace Archipelago.MultiClient.Net.Enums
{
    [Flags]
    public enum SlotType
    {
        Spectator = 0,
        Player = 1 << 0,
        Group = 1 << 1
    }
}