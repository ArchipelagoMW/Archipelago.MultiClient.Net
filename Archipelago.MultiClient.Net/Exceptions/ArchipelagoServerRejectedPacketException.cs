using Archipelago.MultiClient.Net.Enums;
using System;

namespace Archipelago.MultiClient.Net.Exceptions
{
    public class ArchipelagoServerRejectedPacketException : Exception
    {
        public ArchipelagoServerRejectedPacketException(ArchipelagoPacketType faultyPacketType, InvalidPacketErrorType errorType, string message) 
            : base(message)
        {
        }
    }
}
