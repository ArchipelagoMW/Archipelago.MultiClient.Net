using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Archipelago.MultiClient.Net.Exceptions
{
    public class UnknownItemIdException : Exception
    {
        public UnknownItemIdException()
        {
        }

        public UnknownItemIdException(string message) : base(message)
        {
        }

        public UnknownItemIdException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownItemIdException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
