using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class ItemInfoResolverFixture
	{
		[Test]
		public void Get_Item_Name_From_DataPackage_Does_Not_Throw()
		{
			var cache = Substitute.For<IDataPackageCache>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();
			connectionInfo.Game.Returns("TestGame");

			var gameDataLookup = Substitute.For<IGameDataLookup>();
			gameDataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1, "TestItem" } });
			cache.TryGetGameDataFromCache("TestGame", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = gameDataLookup;
				return true;
			});

			var sut = new ItemInfoResolver(cache, connectionInfo);

			var itemName = sut.GetItemName(1);
			Assert.That(itemName, Is.EqualTo("TestItem"));
		}

		[Test]
		public void Get_Item_Name_Returns_Null_For_when_ItemId_Doesnt_Exist()
		{
			var cache = Substitute.For<IDataPackageCache>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var gameDataLookup = Substitute.For<IGameDataLookup>();
			gameDataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1, "TestItem" } });
			cache.TryGetGameDataFromCache("TestGame", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = gameDataLookup;
				return true;
			});

			var sut = new ItemInfoResolver(cache, connectionInfo);

			var itemName = sut.GetItemName(2);
			Assert.That(itemName, Is.Null);
		}

		[Test]
		public void Retrieving_item_name_from_id_use_current_game_as_base()
		{
			var cache = Substitute.For<IDataPackageCache>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var sut = new ItemInfoResolver(cache, connectionInfo);

			var archipelagoDataLookup = Substitute.For<IGameDataLookup>();
			archipelagoDataLookup.Items.Returns(new TwoWayLookup<long, string> { { -100, "ArchipelagoItem" } });
			var game1DataLookup = Substitute.For<IGameDataLookup>();
			game1DataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1337, "Game1Item" } });
			var game2DataLookup = Substitute.For<IGameDataLookup>();
			game2DataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1337, "Game2Item" } });

			cache.TryGetGameDataFromCache("Archipelago", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = archipelagoDataLookup;
				return true;
			});
			cache.TryGetGameDataFromCache("Game1", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = game1DataLookup;
				return true;
			});
			cache.TryGetGameDataFromCache("Game2", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = game2DataLookup;
				return true;
			});

			connectionInfo.Game.Returns("Game2");
			var itemName = sut.GetItemName(-100);

			Assert.That(itemName, Is.EqualTo("ArchipelagoItem"));
		}

		[Test]
		public void Retrieving_item_name_from_id_use_specific_game()
		{
			var cache = Substitute.For<IDataPackageCache>();
			var connectionInfo = Substitute.For<IConnectionInfoProvider>();

			var sut = new ItemInfoResolver(cache, connectionInfo);

			var archipelagoDataLookup = Substitute.For<IGameDataLookup>();
			archipelagoDataLookup.Items.Returns(new TwoWayLookup<long, string> { { -100, "ArchipelagoItem" } });
			var game1DataLookup = Substitute.For<IGameDataLookup>();
			game1DataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1337, "Game1Item" } });
			var game2DataLookup = Substitute.For<IGameDataLookup>();
			game2DataLookup.Items.Returns(new TwoWayLookup<long, string> { { 1337, "Game2Item" } });

			cache.TryGetGameDataFromCache("Archipelago", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = archipelagoDataLookup;
				return true;
			});
			cache.TryGetGameDataFromCache("Game1", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = game1DataLookup;
				return true;
			});
			cache.TryGetGameDataFromCache("Game2", out Arg.Any<IGameDataLookup>()).Returns(x =>
			{
				x[1] = game2DataLookup;
				return true;
			});

			connectionInfo.Game.Returns("Game2");
			var itemName = sut.GetItemName(-100, "Game1");

			Assert.That(itemName, Is.EqualTo("ArchipelagoItem"));
		}
	}
}
