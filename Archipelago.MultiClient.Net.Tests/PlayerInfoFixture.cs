using Archipelago.MultiClient.Net.Helpers;
using NUnit.Framework;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class PlayerInfoFixture
	{
		[Test]
		public void IsRelatedTo_should_work_both_ways()
		{
			var PlayerA = new PlayerInfo { Team = 0, Slot = 1 };
			var PlayerB = new PlayerInfo { Team = 0, Slot = 2 };
			var GroupC  = new PlayerInfo { Team = 0, Slot = 3, GroupMembers = [1,2] };

			Assert.IsTrue(PlayerA.IsRelatedTo(PlayerA));
			Assert.IsTrue(PlayerB.IsRelatedTo(PlayerB));
			Assert.IsTrue(GroupC.IsRelatedTo(GroupC));

			Assert.IsTrue(PlayerA.IsRelatedTo(GroupC));
			Assert.IsTrue(GroupC.IsRelatedTo(PlayerA));

			Assert.IsTrue(PlayerB.IsRelatedTo(GroupC));
			Assert.IsTrue(GroupC.IsRelatedTo(PlayerB));

			Assert.IsFalse(PlayerA.IsRelatedTo(PlayerB));
			Assert.IsFalse(PlayerB.IsRelatedTo(PlayerA));
		}

		[Test]
		public void IsRelatedTo_should_not_work_cross_teams()
		{
			var Player = new PlayerInfo { Team = 0, Slot = 1 };
			var Group = new PlayerInfo { Team = 1, Slot = 2, GroupMembers = [1] };

			Assert.IsFalse(Player.IsRelatedTo(Group));
			Assert.IsFalse(Group.IsRelatedTo(Player));
		}

		[Test]
		public void Equality_should_check_slot_and_team()
		{
			var PlayerA = new PlayerInfo { Team = 0, Slot = 1, Name = "Player", Game = "GameOne" };
			var PlayerB = new PlayerInfo { Team = 0, Slot = 1, Name = "Different Player Name has no impact", GroupMembers = [1], Game = "Game2" };
			var PlayerC = new PlayerInfo { Team = 1, Slot = 1, Name = "Player", Game = "GameOne" };
			var PlayerD = new PlayerInfo { Team = 0, Slot = 2 };

			Assert.That(PlayerA, Is.EqualTo(PlayerB));
			Assert.That(PlayerA, Is.Not.EqualTo(PlayerC));
			Assert.That(PlayerA, Is.Not.EqualTo(PlayerD));
			Assert.That((PlayerInfo)null, Is.EqualTo(null));
		}
	}
}
