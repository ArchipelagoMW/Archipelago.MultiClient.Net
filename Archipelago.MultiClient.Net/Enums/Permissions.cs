using Archipelago.MultiClient.Net.Converters;
using Newtonsoft.Json;
using System;

namespace Archipelago.MultiClient.Net.Enums
{
	/// <summary>
	/// Enum flags that describe the permissions of the currently connected player
	/// </summary>
    [JsonConverter(typeof(PermissionsEnumConverter))]
    [Flags]
    public enum Permissions
    {
		/// <summary>
		/// Permission is not granted
		/// </summary>
        Disabled = 0,
		/// <summary>
		/// Permission is granted to manually execute this
		/// </summary>
		Enabled = 1 << 0,
		/// <summary>
		/// Permission is granted to manually execute this after your goal is completed
		/// </summary>
		Goal = 1 << 1,
		/// <summary>
		/// Will automaticly execute this after your goal is completed, No permission is not granted for manually executing this
		/// </summary>
		Auto = 1 << 2 | Goal,
		/// <summary>
		/// Will automaticly execute this after your goal is completed, Permission is granted to manually execute this
		/// </summary>
		AutoEnabled = Auto | Enabled,
	}
}
