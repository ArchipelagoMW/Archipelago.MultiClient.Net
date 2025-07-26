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
        PrintJSON,
        Connect,
        ConnectUpdate,
        LocationChecks,
        LocationScouts,
		CreateHints,
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
        SetNotify,
        UpdateHint,
        Unknown
    }
}
