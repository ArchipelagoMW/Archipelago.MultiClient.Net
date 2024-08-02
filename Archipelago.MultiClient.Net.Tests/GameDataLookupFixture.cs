using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class GameDataLookupFixture
	{
		[Test]
		public void should_not_crash_on_invalid_gamedata_with_duplicated_ids()
		{
			var data = new GameData
			{
				ItemLookup = new Dictionary<string, long>
				{
					{ "Item1", 100 }, 
					{ "Item2", 100 }, 
					{ "Item3", 102 }
				},
				LocationLookup = new Dictionary<string, long>
				{
					{ "Location1", 1000 },
					{ "Location2", 1001 },
					{ "Location3", 1001 }
				}
			};

			GameDataLookup sut = null;

			Assert.DoesNotThrow(() => sut = new GameDataLookup(data));

			Assert.That(sut, Is.Not.Null);
			Assert.That(sut.Items["Item1"], Is.EqualTo(100));
			Assert.That(sut.Items["Item2"], Is.EqualTo(100));
			Assert.That(sut.Items["Item3"], Is.EqualTo(102));
			Assert.That(sut.Items[100], Is.EqualTo("Item2"));
			Assert.That(sut.Items[102], Is.EqualTo("Item3"));

			Assert.That(sut.Locations["Location1"], Is.EqualTo(1000));
			Assert.That(sut.Locations["Location2"], Is.EqualTo(1001));
			Assert.That(sut.Locations["Location3"], Is.EqualTo(1001));
			Assert.That(sut.Locations[1000], Is.EqualTo("Location1"));
			Assert.That(sut.Locations[1001], Is.EqualTo("Location3"));
		}
	}
}
