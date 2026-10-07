using System;
using System.Runtime.Serialization;

namespace Archipelago.MultiClient.Net.Exceptions
{
    /// <summary>
    ///     The server sent a message larger than <see cref="Helpers.IArchipelagoSocketHelper.MaxMessageSize"/> allows; the connection was closed.
    /// </summary>
    public class ArchipelagoMessageTooLargeException : Exception
    {
        /// <summary>
        ///     The limit the message exceeded, in bytes.
        /// </summary>
        public long MaxMessageSize { get; }

        public ArchipelagoMessageTooLargeException(long maxMessageSize)
            : base($"The server sent a message larger than the {maxMessageSize} bytes allowed; the connection was closed.")
        {
            MaxMessageSize = maxMessageSize;
        }

        public ArchipelagoMessageTooLargeException(string message) : base(message)
        {
        }

        public ArchipelagoMessageTooLargeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ArchipelagoMessageTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
