using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class SayPacket: ArchipelagoPacketBase
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
