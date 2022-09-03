#if NET471
using Archipelago.MultiClient.Net.ConcurrentCollection;
using NUnit.Framework;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    public class ConcurrentCollectionsFixture
    {
        [Test]
        public void ConcurrentQueue_EnqueueSingleItem_ItemIsEnqueued()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);

            Assert.That(queue.Count, Is.EqualTo(1));
        }

        [Test]
        public void ConcurrentQueue_QueueHasItems_ClearQueue_QueueIsEmpty()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);
            queue.Clear();

            Assert.That(queue.IsEmpty, Is.True);
        }

        [Test]
        public void ConcurrentQueue_HasNoItems_GetIsEmpty_ReturnsTrue()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();

            Assert.That(queue.IsEmpty, Is.True);
        }

        [Test]
        public void ConcurrentQueue_HasItems_GetIsEmpty_ReturnsFalse()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);

            Assert.That(queue.IsEmpty, Is.False);
        }

        [Test]
        public void ConcurrentQueue_HasNoItems_TryPeek_ReturnsFalse()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();

            Assert.That(queue.TryPeek(out var item), Is.False);
        }

        [Test]
        public void ConcurrentQueue_HasNoItems_TryPeek_AssignsDefaultValue()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.TryPeek(out var item);

            Assert.That(item, Is.EqualTo(0));
        }

        [Test]
        public void ConcurrentQueue_HasItems_TryPeek_ReturnsTrue()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);

            Assert.That(queue.TryPeek(out var item), Is.True);
        }

        [Test]
        public void ConcurrentQueue_HasItems_TryPeek_AssignsCorrectValue()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);
            queue.TryPeek(out var item);

            Assert.That(item, Is.EqualTo(1));
        }

        [Test]
        public void ConcurrentQueue_HasItems_TryPeek_DoesNotDequeueItem()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);
            queue.TryPeek(out var item);

            Assert.That(queue.Count, Is.EqualTo(1));
        }

        [Test]
        public void ConcurrentQueue_HasNoItems_TryDequeue_ReturnsFalse()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();

            Assert.That(queue.TryDequeue(out var item), Is.False);
        }

        [Test]
        public void ConcurrentQueue_HasNoItems_TryDequeue_AssignsDefaultValue()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.TryDequeue(out var item);

            Assert.That(item, Is.EqualTo(0));
        }

        [Test]
        public void ConcurrentQueue_HasItems_TryDequeue_ReturnsTrue()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);

            Assert.That(queue.TryDequeue(out var item), Is.True);
        }

        [Test]
        public void ConcurrentQueue_HasItems_TryDequeue_AssignsCorrectValue()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);
            queue.TryDequeue(out var item);

            Assert.That(item, Is.EqualTo(1));
        }

        [Test]
        public void ConcurrentQueue_HasItems_TryDequeue_DoesDequeueItem()
        {
            ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
            queue.Enqueue(1);
            queue.TryDequeue(out var item);

            Assert.That(queue.Count, Is.EqualTo(0));
        }
    }
}
#endif