#if NET35
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.MultiClient.Net.ConcurrentCollection
{
    internal class ConcurrentHashSet<T> : IConcurrentHashSet<T>
    {
        private readonly HashSet<T> set = new HashSet<T>();

        private readonly object lockObject = new object();

        public bool TryAdd(T item)
        {
            lock (lockObject)
            {
                if (!Contains(item))
                {
                    set.Add(item);
                    return true;
                }

                return false;
            }
        }
        
        public bool Contains(T item)
        {
            lock (lockObject)
            {
                return set.Contains(item);
            }
        }

        public void UnionWith(T[] otherSet)
        {
            var threadSafeOtherSet = new HashSet<T>(otherSet);

            lock (lockObject)
            {
                set.UnionWith(threadSafeOtherSet);
            }
        }

        public T[] ToArray()
        {
            lock (lockObject)
            {
                return set.ToArray();
            }
        }

        public ReadOnlyCollection<T> AsToReadOnlyCollection()
        {
            return new ReadOnlyCollection<T>(ToArray());
        }

        public ReadOnlyCollection<T> AsToReadOnlyCollectionExcept(IConcurrentHashSet<T> otherSet)
        {
            lock (lockObject)
            {
                var itemsToKeep = new List<T>(set.Count);

                foreach (T item in set)
                {
                    if (!otherSet.Contains(item))
                    {
                        itemsToKeep.Add(item);
                    }
                }

                return new ReadOnlyCollection<T>(itemsToKeep);
            }
        }
    }
}
#endif
