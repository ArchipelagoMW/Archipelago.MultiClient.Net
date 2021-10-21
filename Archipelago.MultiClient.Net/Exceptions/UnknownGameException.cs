using System;
using System.Runtime.Serialization;

namespace Archipelago.MultiClient.Net.Exceptions
{
    [Serializable]
    internal class UnknownGameException : Exception
    {
        public UnknownGameException()
        {
        }

        public UnknownGameException(string message) : base(message)
        {
        }

        public UnknownGameException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownGameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}