using System;

namespace Archipelago.MultiClient.Net.Enums
{
    public enum ArchipelagoPacketType
    {
        RoomInfo,
        ConnectionRefused,
        Connected,
        ReceivedItems,
        LocationInfo,
        RoomUpdate,
        [Obsolete("Print packets are only supported for AP servers up to 0.3.7, use session.MessageLog.OnMessageReceived to receive messages")]
		Print,
        PrintJSON,
        Connect,
        ConnectUpdate,
        LocationChecks,
        LocationScouts,
        StatusUpdate,
        Say,
        GetDataPackage,
        DataPackage,
        Sync,
        Bounced,
        Bounce,
        InvalidPacket,
        Get,
        Retrieved,
        Set,
        SetReply,
        SetNotify
    }
}