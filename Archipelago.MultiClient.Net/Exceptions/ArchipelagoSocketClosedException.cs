using System;
using System.Runtime.Serialization;

namespace Archipelago.MultiClient.Net.Exceptions
{
    public class ArchipelagoSocketClosedException : Exception
    {
        public ArchipelagoSocketClosedException()
        {
        }

        public ArchipelagoSocketClosedException(string message) : base(message)
        {
        }

        public ArchipelagoSocketClosedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ArchipelagoSocketClosedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
