using System.Collections.ObjectModel;

interface IConcurrentHashSet<T>
{
    bool TryAdd(T item);
    bool TryRemove(T item);
    void Clear();
    bool Contains(T item);
    void UnionWith(T[] otherSet);
    T[] ToArray();
    ReadOnlyCollection<T> AsToReadOnlyCollection();
    ReadOnlyCollection<T> AsToReadOnlyCollectionExcept(IConcurrentHashSet<T> otherSet);
}
