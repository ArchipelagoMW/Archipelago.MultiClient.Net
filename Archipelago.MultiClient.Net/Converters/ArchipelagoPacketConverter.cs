using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Archipelago.MultiClient.Net.Converters
{
    public class ArchipelagoPacketConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType.IsAssignableFrom(typeof(ArchipelagoPacketBase));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JObject.Load(reader);

            var commandType = token["cmd"].ToString();
            ArchipelagoPacketType packetType = (ArchipelagoPacketType)Enum.Parse(typeof(ArchipelagoPacketType), commandType);
            ArchipelagoPacketBase ret;
            switch (packetType)
            {
                case ArchipelagoPacketType.RoomInfo:
                    ret = token.ToObject<RoomInfoPacket>();
                    break;
                case ArchipelagoPacketType.ConnectionRefused:
                    ret = token.ToObject<ConnectionRefusedPacket>();
                    break;
                case ArchipelagoPacketType.Connected:
                    ret = token.ToObject<ConnectedPacket>();
                    break;
                case ArchipelagoPacketType.ReceivedItems:
                    ret = token.ToObject<ReceivedItemsPacket>();
                    break;
                case ArchipelagoPacketType.LocationInfo:
                    ret = token.ToObject<LocationInfoPacket>();
                    break;
                case ArchipelagoPacketType.RoomUpdate:
                    ret = token.ToObject<RoomUpdatePacket>();
                    break;
                case ArchipelagoPacketType.Print:
                    ret = token.ToObject<PrintPacket>();
                    break;
                case ArchipelagoPacketType.PrintJSON:
                    ret = token.ToObject<PrintJsonPacket>();
                    break;
                case ArchipelagoPacketType.Connect:
                    ret = token.ToObject<ConnectPacket>();
                    break;
                case ArchipelagoPacketType.LocationChecks:
                    ret = token.ToObject<LocationChecksPacket>();
                    break;
                case ArchipelagoPacketType.LocationScouts:
                    ret = token.ToObject<LocationScoutsPacket>();
                    break;
                case ArchipelagoPacketType.StatusUpdate:
                    ret = token.ToObject<StatusUpdatePacket>();
                    break;
                case ArchipelagoPacketType.Say:
                    ret = token.ToObject<SayPacket>();
                    break;
                case ArchipelagoPacketType.GetDataPackage:
                    ret = token.ToObject<GetDataPackagePacket>();
                    break;
                case ArchipelagoPacketType.DataPackage:
                    ret = token.ToObject<DataPackagePacket>();
                    break;
                default:
                    throw new NotImplementedException("Received an unknown packet.");
            }
            return ret;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}