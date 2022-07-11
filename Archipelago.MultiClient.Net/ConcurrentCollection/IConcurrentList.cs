using System.Collections.ObjectModel;

namespace Archipelago.MultiClient.Net.ConcurrentCollection
{
    interface IConcurrentList<T>
    {
        int Count { get; }
        void Add(T item);
        void Clear();
        ReadOnlyCollection<T> AsReadOnlyCollection();
    }
}