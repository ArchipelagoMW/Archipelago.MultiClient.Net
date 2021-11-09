using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Archipelago.MultiClient.Net.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	[Flags]
	public enum Permissions
	{
		Disabled = 0,
		Enabled = 1 << 0,
		Goal = 1 << 1,
		Auto = 1 << 2 + 1 << 1,
	}
}
