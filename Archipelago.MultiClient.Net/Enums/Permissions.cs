using System;
using Archipelago.MultiClient.Net.Converters;
using Newtonsoft.Json;

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
