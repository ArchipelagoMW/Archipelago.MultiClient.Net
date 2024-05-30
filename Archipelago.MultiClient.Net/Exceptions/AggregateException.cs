#if NET35

using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Exceptions
{
	public class AggregateException : Exception
	{
		public List<Exception> Exceptions { get; } = new List<Exception>();

		public AggregateException(IEnumerable<Exception> exceptions) : base($"AggregateException: {exceptions.Count()} exceptions occurred")
		{
			Exceptions.AddRange(exceptions);
		}
	}
}

#endif
