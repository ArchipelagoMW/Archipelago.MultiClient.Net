using System;
using System.Runtime.Serialization;

namespace Archipelago.MultiClient.Net.Exceptions
{
    class CacheLoadFailureException : Exception
    {
        public CacheLoadFailureException()
        {
        }

        public CacheLoadFailureException(string message) : base(message)
        {
        }

        public CacheLoadFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CacheLoadFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}