using System;

namespace Archipelago.MultiClient.Net.Enums
{
    [Flags]
    public enum ItemFlags
    {
        None = 0,
        Advancement = 1 << 0,
        NeverExclude = 1 << 1,
        Trap = 1 << 2,
    }
}
