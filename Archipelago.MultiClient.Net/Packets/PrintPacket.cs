using Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class PrintPacket: ArchipelagoPacketBase
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
