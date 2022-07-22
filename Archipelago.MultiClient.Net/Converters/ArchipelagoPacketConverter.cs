using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Converters
{
    public class ArchipelagoPacketConverter : JsonConverter
    {
        public static Dictionary<ArchipelagoPacketType, Func<JObject, ArchipelagoPacketBase>> PacketDeserializationMap = new Dictionary<ArchipelagoPacketType, Func<JObject, ArchipelagoPacketBase>>()
        {
            [ArchipelagoPacketType.RoomInfo]          = obj => obj.ToObject<RoomInfoPacket>(),
            [ArchipelagoPacketType.ConnectionRefused] = obj => obj.ToObject<ConnectionRefusedPacket>(),
            [ArchipelagoPacketType.Connected]         = obj => obj.ToObject<ConnectedPacket>(),
            [ArchipelagoPacketType.ReceivedItems]     = obj => obj.ToObject<ReceivedItemsPacket>(),
            [ArchipelagoPacketType.LocationInfo]      = obj => obj.ToObject<LocationInfoPacket>(),
            [ArchipelagoPacketType.RoomUpdate]        = obj => obj.ToObject<RoomUpdatePacket>(),
            [ArchipelagoPacketType.Print]             = obj => obj.ToObject<PrintPacket>(),
            [ArchipelagoPacketType.PrintJSON]         = DeserializePrintJsonPacket,
            [ArchipelagoPacketType.Connect]           = obj => obj.ToObject<ConnectPacket>(),
            [ArchipelagoPacketType.ConnectUpdate]     = obj => obj.ToObject<ConnectUpdatePacket>(),
            [ArchipelagoPacketType.LocationChecks]    = obj => obj.ToObject<LocationChecksPacket>(),
            [ArchipelagoPacketType.LocationScouts]    = obj => obj.ToObject<LocationScoutsPacket>(),
            [ArchipelagoPacketType.StatusUpdate]      = obj => obj.ToObject<StatusUpdatePacket>(),
            [ArchipelagoPacketType.Say]               = obj => obj.ToObject<SayPacket>(),
            [ArchipelagoPacketType.GetDataPackage]    = obj => obj.ToObject<GetDataPackagePacket>(),
            [ArchipelagoPacketType.DataPackage]       = obj => obj.ToObject<DataPackagePacket>(),
            [ArchipelagoPacketType.Bounce]            = obj => obj.ToObject<BouncePacket>(),
            [ArchipelagoPacketType.Bounced]           = obj => obj.ToObject<BouncedPacket>(),
            [ArchipelagoPacketType.InvalidPacket]     = obj => obj.ToObject<InvalidPacketPacket>(),
            [ArchipelagoPacketType.Get]               = obj => obj.ToObject<GetPacket>(),
            [ArchipelagoPacketType.Retrieved]         = obj => obj.ToObject<RetrievedPacket>(),
            [ArchipelagoPacketType.Set]               = obj => obj.ToObject<SetPacket>(),
            [ArchipelagoPacketType.SetNotify]         = obj => obj.ToObject<SetNotifyPacket>(),
            [ArchipelagoPacketType.SetReply]          = obj => obj.ToObject<SetReplyPacket>(),
        };

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType.IsAssignableFrom(typeof(ArchipelagoPacketBase));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JObject.Load(reader);

            var commandType = token["cmd"].ToString();
            ArchipelagoPacketType packetType = (ArchipelagoPacketType)Enum.Parse(typeof(ArchipelagoPacketType), commandType);
            
            ArchipelagoPacketBase ret = null;
            if (PacketDeserializationMap.ContainsKey(packetType))
            {
                ret = PacketDeserializationMap[packetType](token);
            }
            else
            {
                throw new InvalidOperationException("Received an unknown packet.");
            }

            return ret;
        }

        private static ArchipelagoPacketBase DeserializePrintJsonPacket(JObject obj)
        {
            if (obj.TryGetValue("type", out var token))
            {
                if (Enum.TryParse(token.ToString(), out JsonMessageType type))
                {
                    switch (type)
                    {
                        case JsonMessageType.Hint:
                            return obj.ToObject<HintPrintJsonPacket>();
                        case JsonMessageType.ItemSend:
                            return obj.ToObject<ItemPrintJsonPacket>();
                    }
                }
            }

            return obj.ToObject<PrintJsonPacket>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}