using Archipelago.MultiClient.Net.ConcurrentCollection;
using NUnit.Framework;
using System.Linq;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    public class ConcurrentListFixture
    {
        [Test]
        public void ConcurrentList_AddItem_CorrectlyAddsItem()
        {
            var list = new ConcurrentList<int>();
            list.Add(1);

            Assert.That(list.Count, Is.EqualTo(1));
        }

        [Test]
        public void ConcurrentList_Count_ReturnsCorrectCount()
        {
            var list = new ConcurrentList<int>();
            list.Add(1);
            list.Add(1);
            list.Add(1);
            list.Add(1);
            list.Add(1);

            Assert.That(list.Count, Is.EqualTo(5));
        }

        [Test]
        public void ConcurrentList_AsReadOnlyCollection_ReturnsCorrectContents()
        {
            var list = new ConcurrentList<int>();
            list.Add(1);

            Assert.That(list.AsReadOnlyCollection(), Is.EquivalentTo(Enumerable.Range(1,1)));
        }

        [Test]
        public void ConcurrentList_Clear_RemovesAllContents()
        {
            var list = new ConcurrentList<int>();
            list.Add(1);
            list.Add(1);
            list.Add(1);
            list.Add(1);
            list.Clear();

            Assert.That(list.Count, Is.Zero);
        }
    }
}
