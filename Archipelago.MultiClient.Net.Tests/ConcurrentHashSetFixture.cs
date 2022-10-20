using Archipelago.MultiClient.Net.ConcurrentCollection;
using NUnit.Framework;
using System.Linq;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    public class ConcurrentHashSetFixture
    {
        [Test]
        public void ConcurrentHashSet_TryAdd_ItemIsntAlreadyPresent_ReturnsTrue()
        {
            var hashset = new ConcurrentHashSet<int>();
            
            Assert.That(hashset.TryAdd(1), Is.True);
        }

        [Test]
        public void ConcurrentHashSet_TryAdd_ItemIsAlreadyPresent_ReturnsFalse()
        {
            var hashset = new ConcurrentHashSet<int>();
            hashset.TryAdd(1);

            Assert.That(hashset.TryAdd(1), Is.False);
        }

        [Test]
        public void ConcurrentHashSet_Contains_ItemIsntAlreadyPresent_ReturnsFalse()
        {
            var hashset = new ConcurrentHashSet<int>();

            Assert.That(hashset.Contains(1), Is.False);
        }

        [Test]
        public void ConcurrentHashSet_Contains_ItemIsAlreadyPresent_ReturnsTrue()
        {
            var hashset = new ConcurrentHashSet<int>();
            hashset.TryAdd(1);

            Assert.That(hashset.Contains(1), Is.True);
        }

        [Test]
        public void ConcurrentHashSet_ToArray_ReturnsCorrectArrayContents()
        {
            var hashset = new ConcurrentHashSet<int>();
            hashset.TryAdd(1);
            hashset.TryAdd(2);

            Assert.That(hashset.ToArray(), Is.EquivalentTo(Enumerable.Range(1, 2)));
        }

        [Test]
        public void ConcurrentHashSet_UnionWith_OtherSet_ProducesCorrectUnion()
        {
            var hashset = new ConcurrentHashSet<int>();
            hashset.TryAdd(1);
            hashset.UnionWith(new int[] { 1, 2, 3 });

            Assert.That(hashset.ToArray(), Is.EquivalentTo(Enumerable.Range(1,3)));
        }

        [Test]
        public void ConcurrentHashSet_AsToReadOnlyCollection_ProducesCorrectContents()
        {
            var hashset = new ConcurrentHashSet<int>();
            hashset.UnionWith(new int[] { 1, 2, 3 });

            Assert.That(hashset.AsToReadOnlyCollection(), Is.EquivalentTo(Enumerable.Range(1, 3)));
        }

        [Test]
        public void ConcurrentHashSet_AsToReadOnlyCollectionExcept_ProducesCorrectContents()
        {
            var hashset = new ConcurrentHashSet<int>();
            hashset.UnionWith(new int[] { 1, 2, 3 });

            var other = new ConcurrentHashSet<int>();
            other.TryAdd(3);

            Assert.That(hashset.AsToReadOnlyCollectionExcept(other), Is.EquivalentTo(Enumerable.Range(1, 2)));
        }
    }
}
