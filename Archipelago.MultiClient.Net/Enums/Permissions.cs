using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Archipelago.MultiClient.Net.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	[Flags]
	public enum Permissions
	{
		Enabled = 1 << 0,
		Goal = 1 << 1,
		Auto = 1 << 2
	}
}
