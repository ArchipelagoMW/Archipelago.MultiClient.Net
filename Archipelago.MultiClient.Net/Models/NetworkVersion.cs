using Newtonsoft.Json;
using System;

namespace Archipelago.MultiClient.Net.Models
{
    //custom version class as the build in .net version has some deserialization issues on .net core 3.1 or higher
    public class NetworkVersion
    {
        [JsonProperty("major")]
        public int Major { get; set; }

        [JsonProperty("minor")]
        public int Minor { get; set; }

        [JsonProperty("build")]
        public int Build { get; set; }

        [JsonProperty("class")] 
        public string Class => "Version";

        public NetworkVersion()
        {
        }

        public NetworkVersion(int major, int minor, int build)
        {
            Major = major;
            Minor = minor;
            Build = build;
        }

        public NetworkVersion(Version version)
        {
            Major = version.Major;
            Minor = version.Minor;
            Build = version.Build;
        }

        public Version ToVersion() => new Version(Major, Minor, Build);
    }
}
