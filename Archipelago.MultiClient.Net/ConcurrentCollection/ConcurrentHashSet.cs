#if !NET35
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.MultiClient.Net.ConcurrentCollection
{
    internal class ConcurrentHashSet<T> : IConcurrentHashSet<T>
    {
        private readonly ConcurrentDictionary<T, byte> set = new ConcurrentDictionary<T, byte>();

        public bool TryAdd(T item) => set.TryAdd(item, 0);

        public bool Contains(T item) => set.ContainsKey(item);

        public void UnionWith(T[] otherSet)
        {
            foreach (var item in otherSet)
                set.TryAdd(item, 0);
        }

        public T[] ToArray() => set.Keys.ToArray();

        public ReadOnlyCollection<T> AsToReadOnlyCollection() => (ReadOnlyCollection<T>)set.Keys;

        public ReadOnlyCollection<T> AsToReadOnlyCollectionExcept(IConcurrentHashSet<T> otherSet)
        {
            var copy = set.Keys;

            var itemsToKeep = new List<T>(copy.Count);

            foreach (T item in copy)
                if (!otherSet.Contains(item))
                    itemsToKeep.Add(item);

            return new ReadOnlyCollection<T>(itemsToKeep);
        }
    }
}
#endif
