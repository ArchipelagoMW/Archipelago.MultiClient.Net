#if !NET35
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Archipelago.MultiClient.Net.ConcurrentCollection
{
    class ConcurrentList<T> : IConcurrentList<T>
    {
        private readonly ConcurrentDictionary<int, T> list = new ConcurrentDictionary<int, T>();

        public int Count => list.Count;

        public void Add(T item) => list.TryAdd(list.Count, item);

        public void Clear() => list.Clear();

        public ReadOnlyCollection<T> AsReadOnlyCollection() => (ReadOnlyCollection<T>)list.Values;
    }
}
#endif
