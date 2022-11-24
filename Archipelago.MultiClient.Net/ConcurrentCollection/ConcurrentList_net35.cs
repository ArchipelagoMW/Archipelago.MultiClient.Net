#if NET35
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Archipelago.MultiClient.Net.ConcurrentCollection
{
    class ConcurrentList<T> : IConcurrentList<T>
    {
        readonly List<T> list = new List<T>();

        readonly object lockObject = new object();

        public int Count
        {
            get
            {
                lock (lockObject)
                {
                    return list.Count;
                }
            }
        }

        public void Add(T item) {
            lock (lockObject)
            {
                list.Add(item);
            }
        }

        public void Clear()
        {
            lock (lockObject)
            {
                list.Clear();
            }
        }
        
        public ReadOnlyCollection<T> AsReadOnlyCollection()
        {
            lock (lockObject)
            {
                return new ReadOnlyCollection<T>(list.ToArray());
            }
        }
    }
}
#endif
