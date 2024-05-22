using System.Collections;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net
{
	class TwoWayLookup<TA,TB> : IEnumerable<KeyValuePair<TB, TA>>
	{
		readonly Dictionary<TA, TB> aToB = new Dictionary<TA, TB>();
		readonly Dictionary<TB, TA> bToA = new Dictionary<TB, TA>();
		
		public TA this[TB b] => bToA[b];
		public TB this[TA a] => aToB[a];

		public void Add(TA a, TB b)
		{
			aToB[a] = b;
			bToA[b] = a;
		}
		public void Add(TB b, TA a) => Add(a, b);

		public bool TryGetValue(TA a, out TB b) => aToB.TryGetValue(a, out b);
		public bool TryGetValue(TB b, out TA a) => bToA.TryGetValue(b, out a);

		public IEnumerator<KeyValuePair<TB, TA>> GetEnumerator() => bToA.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
