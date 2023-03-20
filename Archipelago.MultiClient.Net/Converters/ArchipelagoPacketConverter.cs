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
        static readonly Dictionary<ArchipelagoPacketType, Func<JObject, ArchipelagoPacketBase>> PacketDeserializationMap = new Dictionary<ArchipelagoPacketType, Func<JObject, ArchipelagoPacketBase>>()
        {
            [ArchipelagoPacketType.RoomInfo]          = obj => obj.ToObject<RoomInfoPacket>(),
            [ArchipelagoPacketType.ConnectionRefused] = obj => obj.ToObject<ConnectionRefusedPacket>(),
            [ArchipelagoPacketType.Connected]         = obj => obj.ToObject<ConnectedPacket>(),
            [ArchipelagoPacketType.ReceivedItems]     = obj => obj.ToObject<ReceivedItemsPacket>(),
            [ArchipelagoPacketType.LocationInfo]      = obj => obj.ToObject<LocationInfoPacket>(),
            [ArchipelagoPacketType.RoomUpdate]        = obj => obj.ToObject<RoomUpdatePacket>(),
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

            ArchipelagoPacketBase packet;
			if (EnumTryParse(commandType, out ArchipelagoPacketType packetType) && PacketDeserializationMap.ContainsKey(packetType))
				packet = PacketDeserializationMap[packetType](token);
			else
				packet = new UnknownPacket();

	        packet.jobject = token;

	        return packet;
		}

        static ArchipelagoPacketBase DeserializePrintJsonPacket(JObject obj)
        {
            if (obj.TryGetValue("type", out var token))
            {
                if (EnumTryParse(token.ToString(), out JsonMessageType type))
                {
                    switch (type)
                    {
                        case JsonMessageType.Hint:
                            return obj.ToObject<HintPrintJsonPacket>();
                        case JsonMessageType.ItemSend:
                            return obj.ToObject<ItemPrintJsonPacket>();
                        case JsonMessageType.Countdown:
	                        return obj.ToObject<CountdownPrintJsonPacket>();
					}
                }

                obj["type"] = null;
            }

            return obj.ToObject<PrintJsonPacket>();
        }


        static bool EnumTryParse<TEnum>(string value, out TEnum result) where TEnum : struct, IConvertible
        {
#if NET35
			if (value == null || !Enum.IsDefined(typeof(TEnum), value))
            {
                result = default;
                return false;
            }

            result = (TEnum)Enum.Parse(typeof(TEnum), value);
            return true;
#else
	        return Enum.TryParse(value, out result);
#endif
        }


		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}