using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Archipelago.MultiClient.Net.Packets
{
    public class PrintJsonPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.PrintJSON;

        [JsonProperty("data")]
        public JsonMessagePart[] Data { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public JsonMessageType? MessageType { get; set; }
    }

    public class ItemPrintJsonPacket : PrintJsonPacket
    {
        [JsonProperty("receiving")]
        public int ReceivingPlayer { get; set; }

        [JsonProperty("item")]
        public NetworkItem Item { get; set; }
    }

    public class ItemCheatPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("receiving")]
	    public int ReceivingPlayer { get; set; }

	    [JsonProperty("item")]
	    public NetworkItem Item { get; set; }
		
	    [JsonProperty("team")]
	    public int Team { get; set; }
	}

	public class HintPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("receiving")]
	    public int ReceivingPlayer { get; set; }

	    [JsonProperty("item")]
	    public NetworkItem Item { get; set; }

		[JsonProperty("found")]
        public bool? Found { get; set; }
    }

    public class CountdownPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("countdown")]
	    public int RemainingSeconds { get; set; }
	}

    public class JoinPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("team")]
	    public int Team { get; set; }

	    [JsonProperty("slot")]
	    public int Slot { get; set; }

	    [JsonProperty("tags")]
	    public string[] Tags { get; set; }
	}

    public class LeavePrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("team")]
	    public int Team { get; set; }

	    [JsonProperty("slot")]
	    public int Slot { get; set; }
    }

    public class ChatPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("team")]
	    public int Team { get; set; }

	    [JsonProperty("slot")]
	    public int Slot { get; set; }

	    [JsonProperty("message")]
	    public string Message { get; set; }
}

    public class ServerChatPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("message")]
	    public string Message { get; set; }
	}

    public class TutorialPrintJsonPacket : PrintJsonPacket
    {
    }

    public class TagsChangedPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("tags")]
	    public string[] Tags { get; set; }
	}

    public class CommandResultPrintJsonPacket : PrintJsonPacket
    {
    }

    public class AdminCommandResultPrintJsonPacket : PrintJsonPacket
    {
    }

    public class GoalPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("team")]
	    public int Team { get; set; }

	    [JsonProperty("slot")]
	    public int Slot { get; set; }
	}

    public class ReleasePrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("team")]
	    public int Team { get; set; }

	    [JsonProperty("slot")]
	    public int Slot { get; set; }
	}

    public class CollectPrintJsonPacket : PrintJsonPacket
    {
	    [JsonProperty("team")]
	    public int Team { get; set; }

	    [JsonProperty("slot")]
	    public int Slot { get; set; }
	}
}
