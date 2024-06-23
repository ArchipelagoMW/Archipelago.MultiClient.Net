using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
    class RoomStateHelperFixture
    {
        public static TestCaseData[] RoomStateHelperTests =>
            new TestCaseData[] {
                // HintCostPercentage
                RoomStateHelperTest.Create(
                    "Should_get_and_update_hint_cost_percentage",
                    new RoomInfoPacket { HintCostPercentage = 99 },
                    new RoomUpdatePacket { HintCostPercentage = 777 },
                    s => s.HintCostPercentage, 99, 777),
                RoomStateHelperTest.Create(
					"Should_not_update_hint_cost_percentage_when_its_not_provided",
                    new RoomInfoPacket { HintCostPercentage = 99 },
                    new RoomUpdatePacket { HintCostPercentage = null },
                    s => s.HintCostPercentage, 99, 99),
                RoomStateHelperTest.Create(
					"Should_update_hint_cost_percentage_to_0_when_its_provided",
                    new RoomInfoPacket { HintCostPercentage = 99 },
                    new RoomUpdatePacket { HintCostPercentage = 0 },
                    s => s.HintCostPercentage, 99, 0),

                // HintCost
                RoomStateHelperTest.Create(
	                "Should_get_and_update_hint_cost",
	                new RoomInfoPacket { HintCostPercentage = 40 },
	                new ConnectedPacket { LocationsChecked = new long[100], MissingChecks = new long[50] },
	                s => s.HintCost, 0, 60),
                RoomStateHelperTest.Create(
	                "Should_not_update_hint_cost_when_its_not_provided",
	                new RoomInfoPacket { HintCostPercentage = 40 },
					new ConnectedPacket { LocationsChecked = new long[100], MissingChecks = new long[50] },
	                new RoomUpdatePacket { HintCostPercentage = null },
	                s => s.HintCost, 0, 60, 60),
                RoomStateHelperTest.Create(
	                "Should_update_hint_cost_to_0_when_its_provided",
	                new RoomInfoPacket { HintCostPercentage = 40 },
	                new ConnectedPacket { LocationsChecked = new long[100], MissingChecks = new long[50] },
					new RoomUpdatePacket { HintCostPercentage = 0 },
	                s => s.HintCost, 0, 60, 0),

                // LocationCheckPoints
                RoomStateHelperTest.Create(
                    "Should_get_and_update_location_check_points",
                    new RoomInfoPacket { LocationCheckPoints = 1 },
                    new RoomUpdatePacket { LocationCheckPoints = 5 },
                    s => s.LocationCheckPoints, 1, 5),
                RoomStateHelperTest.Create(
                    "Should_not_update_location_check_points_when_its_not_provided",
                    new RoomInfoPacket { LocationCheckPoints = 2 },
                    new RoomUpdatePacket { LocationCheckPoints = null },
                    s => s.LocationCheckPoints, 2, 2),
                RoomStateHelperTest.Create(
                    "Should_update_location_check_points_to_0_when_its_provided",
                    new RoomInfoPacket { LocationCheckPoints = 3 },
                    new RoomUpdatePacket { LocationCheckPoints = 0 },
                    s => s.LocationCheckPoints, 3, 0),

                // HintPoints
                RoomStateHelperTest.Create(
                    "Should_update_hint_points",
                    new RoomUpdatePacket { HintPoints = 1337 },
                    new RoomUpdatePacket { HintPoints = 1350 },
                    s => s.HintPoints, 1337, 1350),
                RoomStateHelperTest.Create(
                    "Should_not_update_hint_points_when_its_not_provided",
                    new RoomUpdatePacket { HintPoints = 1337 },
                    new RoomUpdatePacket { HintPoints = null },
                    s => s.HintPoints, 1337, 1337),
                RoomStateHelperTest.Create(
                    "Should_update_hint_points_to_0_when_its_provided",
                    new RoomUpdatePacket { HintPoints = 1337 },
                    new RoomUpdatePacket { HintPoints = 0 },
                    s => s.HintPoints, 1337, 0),

                // Tags
                RoomStateHelperTest.Create(
                    "Should_update_tags",
                    new RoomInfoPacket { Tags = new []{ "Tag1" } },
                    new RoomUpdatePacket { Tags = new []{ "Tag2" } },
                    s => s.ServerTags, new List<string> { "Tag1" }.AsReadOnly(), new List<string> { "Tag2" }.AsReadOnly()),
                RoomStateHelperTest.Create(
                    "Should_not_update_tags_when_its_not_provided",
                    new RoomInfoPacket { Tags = new []{ "Tag1" } },
                    new RoomUpdatePacket { Tags = null },
                    s => s.ServerTags, new List<string> { "Tag1" }.AsReadOnly(), new List<string> { "Tag1" }.AsReadOnly()),
                RoomStateHelperTest.Create(
                    "Should_update_tags_to_empty_when_its_provided",
                    new RoomInfoPacket { Tags = new []{ "Tag1" } },
                    new RoomUpdatePacket { Tags = Array.Empty<string>() },
                    s => s.ServerTags, new List<string> { "Tag1" }.AsReadOnly(), new List<string>().AsReadOnly()),

                // Password
                RoomStateHelperTest.Create(
                    "Should_get_and_update_password",
                    new RoomInfoPacket { Password = false },
                    new RoomUpdatePacket { Password = true },
                    s => s.HasPassword, false, true),
                RoomStateHelperTest.Create(
                    "Should_not_update_password_when_its_not_provided",
                    new RoomInfoPacket { Password = true },
                    new RoomUpdatePacket { Password = null },
                    s => s.HasPassword, true, true),
                RoomStateHelperTest.Create(
                    "Should_update_password_to_false_when_its_provided",
                    new RoomInfoPacket { Password = true },
                    new RoomUpdatePacket { Password = false },
                    s => s.HasPassword, true, false),

                // Release Permissions
                RoomStateHelperTest.Create(
	                "Should_get_and_update_release_permissions",
	                new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Enabled } } },
	                new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Goal } } },
	                s => s.ReleasePermissions, Permissions.Enabled, Permissions.Goal),
                RoomStateHelperTest.Create(
					"Should_not_update_release_permissions_when_its_not_provided",
	                new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Disabled } } },
	                new RoomUpdatePacket { Permissions = null },
	                s => s.ReleasePermissions, Permissions.Disabled, Permissions.Disabled),
                RoomStateHelperTest.Create(
					"Should_update_release_permissions_to_false_when_its_provided",
	                new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Auto } } },
	                new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Enabled } } },
	                s => s.ReleasePermissions, Permissions.Auto, Permissions.Enabled),

				//forward compatibility
				RoomStateHelperTest.Create(
					"Should_get_and_update_release_permissions_from_forfeit_permissions",
	                new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Enabled } } },
	                new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Goal } } },
	                s => s.ReleasePermissions, Permissions.Enabled, Permissions.Goal),
				RoomStateHelperTest.Create(
	                "Should_update_release_permissions_to_false_when_its_provided_as_forfeit_permission",
	                new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Auto } } },
	                new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Enabled } } },
	                s => s.ReleasePermissions, Permissions.Auto, Permissions.Enabled),
				RoomStateHelperTest.Create(
	                "Should_get_and_update_forfeit_permissions_from_release_permissions",
	                new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Enabled } } },
	                new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Goal } } },
	                s => s.ReleasePermissions, Permissions.Enabled, Permissions.Goal),
				RoomStateHelperTest.Create(
	                "Should_update_forfeit_permissions_to_false_when_its_provided_as_release_permission",
	                new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Auto } } },
	                new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "release", Permissions.Enabled } } },
	                s => s.ReleasePermissions, Permissions.Auto, Permissions.Enabled),
				RoomStateHelperTest.Create(
	                "Should_get_and_update_release_permissions_from_release_permissions_over_forfeit_permissions",
	                new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> {
		                { "forfeit", Permissions.Auto },
		                { "release", Permissions.Enabled }
					} },
	                new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> {
			            { "forfeit", Permissions.Disabled },
		                { "release", Permissions.Auto }
					} },
					s => s.ReleasePermissions, Permissions.Enabled, Permissions.Auto),
                // Forfeit Permissions
                RoomStateHelperTest.Create(
                    "Should_get_and_update_forfeit_permissions",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Enabled } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Goal } } },
                    s => s.ForfeitPermissions, Permissions.Enabled, Permissions.Goal),
                RoomStateHelperTest.Create(
                    "Should_not_update_forfeit_permissions_when_its_not_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Disabled } } },
                    new RoomUpdatePacket { Permissions = null },
                    s => s.ForfeitPermissions, Permissions.Disabled, Permissions.Disabled),
                RoomStateHelperTest.Create(
                    "Should_update_forfeit_permissions_to_false_when_its_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Auto } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Enabled } } },
                    s => s.ForfeitPermissions, Permissions.Auto, Permissions.Enabled),


                // Collect Permissions
                RoomStateHelperTest.Create(
                    "Should_get_and_collect_permissions",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Enabled } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Goal } } },
                    s => s.CollectPermissions, Permissions.Enabled, Permissions.Goal),
                RoomStateHelperTest.Create(
                    "Should_not_update_collect_permissions_when_its_not_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Disabled } } },
                    new RoomUpdatePacket { Permissions = null },
                    s => s.CollectPermissions, Permissions.Disabled, Permissions.Disabled),
                RoomStateHelperTest.Create(
                    "Should_update_collect_permissions_to_enabled_when_its_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Auto } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Enabled } } },
                    s => s.CollectPermissions, Permissions.Auto, Permissions.Enabled),

                // Remaining Permissions
                RoomStateHelperTest.Create(
                    "Should_get_and_update_remaining_permissions",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Enabled } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Goal } } },
                    s => s.RemainingPermissions, Permissions.Enabled, Permissions.Goal),
                RoomStateHelperTest.Create(
                    "Should_not_update_remaining_permissions_when_its_not_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Disabled } } },
                    new RoomUpdatePacket { Permissions = null },
                    s => s.RemainingPermissions, Permissions.Disabled, Permissions.Disabled),
                RoomStateHelperTest.Create(
                    "Should_update_remaining_permissions_to_enabled_when_its_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Auto } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Enabled } } },
                    s => s.RemainingPermissions, Permissions.Auto, Permissions.Enabled),

                // Version
                RoomStateHelperTest.Create(
                    "Should_read_version",
                    new RoomInfoPacket { Version = new NetworkVersion(1, 2, 3) },
                    s => s.Version, new Version(1, 2, 3)),

                // Generator Version
                RoomStateHelperTest.Create(
	                "Should_read_generator_version",
	                new RoomInfoPacket { GeneratorVersion = new NetworkVersion(5, 6, 9) },
	                s => s.GeneratorVersion, new Version(5, 6, 9)),

                // Seed
                RoomStateHelperTest.Create(
                    "Should_read_seed",
                    new RoomInfoPacket { SeedName = "436191FC1F3CF6410B92" },
                    s => s.Seed, "436191FC1F3CF6410B92"),

                // Time
                RoomStateHelperTest.Create(
                    "Should_read_time",
                    new RoomInfoPacket { Timestamp = new DateTime(2000, 1, 2, 3, 4, 5).ToUnixTimeStamp() },
                    s => s.RoomInfoSendTime, new DateTime(2000, 1, 2, 3, 4, 5))
            };

		[TestCaseSource(nameof(RoomStateHelperTests))]
		public void Should_update_values<T>(
	        ArchipelagoPacketBase firstPacket, ArchipelagoPacketBase secondPacket, ArchipelagoPacketBase thirdPacket,
	        Func<RoomStateHelper, object> getValue,
	        T expectedValueAfterFirstPacket, T expectedValueAfterSecondPacket, T expectedValueAfterThirdPacket)
        {
	        var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var locationHelper = Substitute.For<ILocationCheckHelper>();
	        locationHelper.AllLocations.Returns(new ReadOnlyCollection<long>(new long[150]));

	        var sut = new RoomStateHelper(socket, locationHelper);

	        Assert.That(getValue(sut), Is.EqualTo(default(T)),
		        $"initial value before the first RoomInfoPacket is received should be {default(T)} but is {getValue(sut)}");

	        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(firstPacket);

	        Assert.That(getValue(sut), Is.EqualTo(expectedValueAfterFirstPacket),
		        $"The initial value after the first packet should be {expectedValueAfterFirstPacket} but is {getValue(sut)}");

	        if (secondPacket != null)
	        {
		        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(secondPacket);

		        Assert.That(getValue(sut), Is.EqualTo(expectedValueAfterSecondPacket),
			        $"The value after the second packet should be {expectedValueAfterSecondPacket} but is {getValue(sut)}");
	        }

	        if (thirdPacket != null)
	        {
		        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(thirdPacket);

		        Assert.That(getValue(sut), Is.EqualTo(expectedValueAfterThirdPacket),
			        $"The value after the third packet should be {expectedValueAfterThirdPacket} but is {getValue(sut)}");
	        }
		}
	}

    class RoomStateHelperTestBase : TestCaseData
    {
        public RoomStateHelperTestBase(
            string testName,
            ArchipelagoPacketBase firstPacket, ArchipelagoPacketBase secondPacket, ArchipelagoPacketBase thirdPacket,
			Func<RoomStateHelper, object> getValue,
            object expectedValueAfterFirstPacket, object expectedValueAfterSecondPacket, object expectedValueAfterThirdPacket)
            : base(firstPacket, secondPacket, thirdPacket, getValue, 
	            expectedValueAfterFirstPacket, expectedValueAfterSecondPacket, expectedValueAfterThirdPacket)
        {
	        TestName = testName;
        }
    }

    static class RoomStateHelperTest
    {
	    public static TestCaseData Create<T>(
		    string testName,
		    ArchipelagoPacketBase firstPacket, ArchipelagoPacketBase secondPacket, ArchipelagoPacketBase thirdPacket,
			Func<RoomStateHelper, T> getValue,
		    T expectedValueAfterFirstPacket, T expectedValueAfterSecondPacket, T expectedValueAfterThirdPacket) =>
				new RoomStateHelperTestBase(testName, firstPacket, secondPacket, thirdPacket, s => getValue(s),
					expectedValueAfterFirstPacket, expectedValueAfterSecondPacket, expectedValueAfterThirdPacket);

	    public static TestCaseData Create<T>(
			string testName,
            ArchipelagoPacketBase firstPacket, ArchipelagoPacketBase secondPacket,
            Func<RoomStateHelper, T> getValue,
            T expectedValueAfterFirstPacket, T expectedValueAfterSecondPacket) =>
				Create(testName, firstPacket, secondPacket, null, getValue, 
					expectedValueAfterFirstPacket, expectedValueAfterSecondPacket, default);

	    public static TestCaseData Create<T>(
			string testName,
            ArchipelagoPacketBase firstPacket,
            Func<RoomStateHelper, T> getValue,
            T expectedValueAfterFirstPacket) =>
				Create(testName, firstPacket, null, getValue, expectedValueAfterFirstPacket, default);
    }
}
