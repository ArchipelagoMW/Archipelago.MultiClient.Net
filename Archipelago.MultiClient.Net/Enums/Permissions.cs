using Archipelago.MultiClient.Net.Converters;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;

namespace Archipelago.MultiClient.Net.Enums
{
    [JsonConverter(typeof(PermissionsEnumConverter))]
    [Flags]
    public enum Permissions
    {
        Disabled = 0,
        Enabled = 1 << 0,
        Goal = 1 << 1,
        Auto = 1 << 2 | Goal,
    }
}
