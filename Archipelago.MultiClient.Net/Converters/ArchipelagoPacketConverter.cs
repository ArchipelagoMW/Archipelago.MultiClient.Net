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
            [ArchipelagoPacketType.RoomInfo]          = (JObject obj) => obj.ToObject<RoomInfoPacket>(),
            [ArchipelagoPacketType.ConnectionRefused] = (JObject obj) => obj.ToObject<ConnectionRefusedPacket>(),
            [ArchipelagoPacketType.Connected]         = (JObject obj) => obj.ToObject<ConnectedPacket>(),
            [ArchipelagoPacketType.ReceivedItems]     = (JObject obj) => obj.ToObject<ReceivedItemsPacket>(),
            [ArchipelagoPacketType.LocationInfo]      = (JObject obj) => obj.ToObject<LocationInfoPacket>(),
            [ArchipelagoPacketType.RoomUpdate]        = (JObject obj) => obj.ToObject<RoomUpdatePacket>(),
            [ArchipelagoPacketType.Print]             = (JObject obj) => obj.ToObject<PrintPacket>(),
            [ArchipelagoPacketType.PrintJSON]         = (JObject obj) => obj.ToObject<PrintJsonPacket>(),
            [ArchipelagoPacketType.Connect]           = (JObject obj) => obj.ToObject<ConnectPacket>(),
            [ArchipelagoPacketType.LocationChecks]    = (JObject obj) => obj.ToObject<LocationChecksPacket>(),
            [ArchipelagoPacketType.LocationScouts]    = (JObject obj) => obj.ToObject<LocationScoutsPacket>(),
            [ArchipelagoPacketType.StatusUpdate]      = (JObject obj) => obj.ToObject<StatusUpdatePacket>(),
            [ArchipelagoPacketType.Say]               = (JObject obj) => obj.ToObject<SayPacket>(),
            [ArchipelagoPacketType.GetDataPackage]    = (JObject obj) => obj.ToObject<GetDataPackagePacket>(),
            [ArchipelagoPacketType.DataPackage]       = (JObject obj) => obj.ToObject<DataPackagePacket>(),
            [ArchipelagoPacketType.Bounce]            = (JObject obj) => obj.ToObject<BouncePacket>(),
            [ArchipelagoPacketType.Bounced]           = (JObject obj) => obj.ToObject<BouncedPacket>(),
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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}