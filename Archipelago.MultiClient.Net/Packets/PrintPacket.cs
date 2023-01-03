using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Archipelago.MultiClient.Net.Packets
{
	[Obsolete("Print packets are only supported for AP servers up to 0.3.7, use session.MessageLog.OnMessageReceived to receive messages")]
    public class PrintPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Print;

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
