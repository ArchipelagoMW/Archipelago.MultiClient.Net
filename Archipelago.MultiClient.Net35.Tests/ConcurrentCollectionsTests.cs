using Archipelago.MultiClient.Net.ConcurrentCollection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Archipelago.MultiClient.Net35.Tests
{
    [TestFixture]
    public class ConcurrentCollectionsFixture
    {
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
    }
}
