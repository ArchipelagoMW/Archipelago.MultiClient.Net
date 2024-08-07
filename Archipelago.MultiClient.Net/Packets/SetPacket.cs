﻿using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Archipelago.MultiClient.Net.Packets
{
    public class SetPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Set;

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("default")]
        public JToken DefaultValue { get; set; }

        [JsonProperty("operations")]
        public OperationSpecification[] Operations { get; set; }

        [JsonProperty("want_reply")]
        public bool WantReply { get; set; }

        [JsonExtensionData]
		public Dictionary<string, JToken> AdditionalArguments { get; set; }

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context) => AdditionalArguments?.Remove("cmd");
    }
}
