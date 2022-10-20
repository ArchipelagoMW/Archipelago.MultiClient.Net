#if NET35
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.ConcurrentCollection
{
    internal class ConcurrentQueue<T>
    {
        public int Count => queue.Count;

        private readonly Queue<T> queue = new Queue<T>();

        private readonly object lockObject = new object();

        public bool IsEmpty
        {
            get
            {
                lock (lockObject)
                {
                    return queue.Count == 0;
                }
            }
        }

        public bool TryPeek(out T item)
        {
            lock (lockObject)
            {
                if (IsEmpty)
                {
                    item = default;
                    return false;
                }

                item = queue.Peek();
                return true;
            }
        }

        public bool TryDequeue(out T item)
        {
            lock (lockObject)
            {
                if (IsEmpty)
                {
                    item = default;
                    return false;
                }

                item = queue.Dequeue();
                return true;
            }
        }

        public void Enqueue(T item)
        {
            lock (lockObject)
            {
                queue.Enqueue(item);
            }
        }

        public void Clear()
        {
            lock (lockObject)
            {
                queue.Clear();
            }
        }
    }
}
#endif