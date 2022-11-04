using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class RoomStateHelperFixture
    {
        public static TestCaseData[] RoomStateHelperTests =>
            new TestCaseData[] {
                // HintCost
                new RoomStateHelperTest<int>(
                    "Should_get_and_update_hint_cost",
                    new RoomInfoPacket { HintCost = 99 },
                    new RoomUpdatePacket { HintCost = 777 },
                    s => s.HintCost, 99, 777),
                new RoomStateHelperTest<int>(
                    "Should_not_update_hint_cost_when_its_not_provided",
                    new RoomInfoPacket { HintCost = 99 },
                    new RoomUpdatePacket { HintCost = null },
                    s => s.HintCost, 99, 99),
                new RoomStateHelperTest<int>(
                    "Should_update_hint_cost_to_0_when_its_provided",
                    new RoomInfoPacket { HintCost = 99 },
                    new RoomUpdatePacket { HintCost = 0 },
                    s => s.HintCost, 99, 0),

                // LocationCheckPoints
                new RoomStateHelperTest<int>(
                    "Should_get_and_update_location_check_points",
                    new RoomInfoPacket { LocationCheckPoints = 1 },
                    new RoomUpdatePacket { LocationCheckPoints = 5 },
                    s => s.LocationCheckPoints, 1, 5),
                new RoomStateHelperTest<int>(
                    "Should_not_update_location_check_points_when_its_not_provided",
                    new RoomInfoPacket { LocationCheckPoints = 2 },
                    new RoomUpdatePacket { LocationCheckPoints = null },
                    s => s.LocationCheckPoints, 2, 2),
                new RoomStateHelperTest<int>(
                    "Should_update_location_check_points_to_0_when_its_provided",
                    new RoomInfoPacket { LocationCheckPoints = 3 },
                    new RoomUpdatePacket { LocationCheckPoints = 0 },
                    s => s.LocationCheckPoints, 3, 0),

                // HintPoints
                new RoomStateHelperTest<int>(
                    "Should_update_hint_points",
                    new RoomUpdatePacket { HintPoints = 1337 },
                    new RoomUpdatePacket { HintPoints = 1350 },
                    s => s.HintPoints, 1337, 1350),
                new RoomStateHelperTest<int>(
                    "Should_not_update_hint_points_when_its_not_provided",
                    new RoomUpdatePacket { HintPoints = 1337 },
                    new RoomUpdatePacket { HintPoints = null },
                    s => s.HintPoints, 1337, 1337),
                new RoomStateHelperTest<int>(
                    "Should_update_hint_points_to_0_when_its_provided",
                    new RoomUpdatePacket { HintPoints = 1337 },
                    new RoomUpdatePacket { HintPoints = 0 },
                    s => s.HintPoints, 1337, 0),

                // Tags
                new RoomStateHelperTest<IReadOnlyCollection<string>>(
                    "Should_update_tags",
                    new RoomInfoPacket { Tags = new []{ "Tag1" } },
                    new RoomUpdatePacket { Tags = new []{ "Tag2" } },
                    s => s.ServerTags, new List<string> { "Tag1" }.AsReadOnly(), new List<string> { "Tag2" }.AsReadOnly()),
                new RoomStateHelperTest<IReadOnlyCollection<string>>(
                    "Should_not_update_tags_when_its_not_provided",
                    new RoomInfoPacket { Tags = new []{ "Tag1" } },
                    new RoomUpdatePacket { Tags = null },
                    s => s.ServerTags, new List<string> { "Tag1" }.AsReadOnly(), new List<string> { "Tag1" }.AsReadOnly()),
                new RoomStateHelperTest<IReadOnlyCollection<string>>(
                    "Should_update_tags_to_empty_when_its_provided",
                    new RoomInfoPacket { Tags = new []{ "Tag1" } },
                    new RoomUpdatePacket { Tags = Array.Empty<string>() },
                    s => s.ServerTags, new List<string> { "Tag1" }.AsReadOnly(), new List<string>().AsReadOnly()),

                // Password
                new RoomStateHelperTest<bool>(
                    "Should_get_and_update_password",
                    new RoomInfoPacket { Password = false },
                    new RoomUpdatePacket { Password = true },
                    s => s.HasPassword, false, true),
                new RoomStateHelperTest<bool>(
                    "Should_not_update_password_when_its_not_provided",
                    new RoomInfoPacket { Password = true },
                    new RoomUpdatePacket { Password = null },
                    s => s.HasPassword, true, true),
                new RoomStateHelperTest<bool>(
                    "Should_update_password_to_false_when_its_provided",
                    new RoomInfoPacket { Password = true },
                    new RoomUpdatePacket { Password = false },
                    s => s.HasPassword, true, false),

                // Forfeit Permissions
                new RoomStateHelperTest<Permissions>(
                    "Should_get_and_update_forfeit_permissions",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Enabled } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Goal } } },
                    s => s.ForfeitPermissions, Permissions.Enabled, Permissions.Goal),
                new RoomStateHelperTest<Permissions>(
                    "Should_not_update_forfeit_permissions_when_its_not_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Disabled } } },
                    new RoomUpdatePacket { Permissions = null },
                    s => s.ForfeitPermissions, Permissions.Disabled, Permissions.Disabled),
                new RoomStateHelperTest<Permissions>(
                    "Should_update_forfeit_permissions_to_false_when_its_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Auto } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "forfeit", Permissions.Enabled } } },
                    s => s.ForfeitPermissions, Permissions.Auto, Permissions.Enabled),

                // Collect Permissions
                new RoomStateHelperTest<Permissions>(
                    "Should_get_and_collect_permissions",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Enabled } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Goal } } },
                    s => s.CollectPermissions, Permissions.Enabled, Permissions.Goal),
                new RoomStateHelperTest<Permissions>(
                    "Should_not_update_collect_permissions_when_its_not_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Disabled } } },
                    new RoomUpdatePacket { Permissions = null },
                    s => s.CollectPermissions, Permissions.Disabled, Permissions.Disabled),
                new RoomStateHelperTest<Permissions>(
                    "Should_update_collect_permissions_to_enabled_when_its_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Auto } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "collect", Permissions.Enabled } } },
                    s => s.CollectPermissions, Permissions.Auto, Permissions.Enabled),

                // Remaining Permissions
                new RoomStateHelperTest<Permissions>(
                    "Should_get_and_update_remaining_permissions",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Enabled } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Goal } } },
                    s => s.RemainingPermissions, Permissions.Enabled, Permissions.Goal),
                new RoomStateHelperTest<Permissions>(
                    "Should_not_update_remaining_permissions_when_its_not_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Disabled } } },
                    new RoomUpdatePacket { Permissions = null },
                    s => s.RemainingPermissions, Permissions.Disabled, Permissions.Disabled),
                new RoomStateHelperTest<Permissions>(
                    "Should_update_remaining_permissions_to_enabled_when_its_provided",
                    new RoomInfoPacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Auto } } },
                    new RoomUpdatePacket { Permissions = new Dictionary<string, Permissions> { { "remaining", Permissions.Enabled } } },
                    s => s.RemainingPermissions, Permissions.Auto, Permissions.Enabled),

                // Version
                new RoomStateHelperTest<Version>(
                    "Should_read_version",
                    new RoomInfoPacket { Version = new NetworkVersion(1, 2, 3) },
                    s => s.Version, new Version(1, 2, 3)),

                // Seed
                new RoomStateHelperTest<string>(
                    "Should_read_seed",
                    new RoomInfoPacket { SeedName = "436191FC1F3CF6410B92" },
                    s => s.Seed, "436191FC1F3CF6410B92"),

                // Time
                new RoomStateHelperTest<DateTime>(
                    "Should_read_time",
                    new RoomInfoPacket { Timestamp = new DateTime(2000, 1, 2, 3, 4, 5).ToUnixTimeStamp() },
                    s => s.RoomInfoSendTime, new DateTime(2000, 1, 2, 3, 4, 5))
            };

        [TestCaseSource(nameof(RoomStateHelperTests))]
        public void Should_update_values<T>(
            ArchipelagoPacketBase firstPacket, ArchipelagoPacketBase secondPacket,
            Func<RoomStateHelper, object> getValue,
            T expectedValueAfterFirstPacket, T expectedValueAfterSecondPacket)
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new RoomStateHelper(socket);

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
        }
    }

    internal class RoomStateHelperTest : TestCaseData
    {
        public RoomStateHelperTest(
            ArchipelagoPacketBase firstPacket, ArchipelagoPacketBase secondPacket,
            Func<RoomStateHelper, object> getValue,
            object expectedValueAfterFirstPacket, object expectedValueAfterSecondPacket)
            : base(firstPacket, secondPacket, getValue, expectedValueAfterFirstPacket, expectedValueAfterSecondPacket)
        {
        }
    }

    internal class RoomStateHelperTest<T> : RoomStateHelperTest
    {
        public RoomStateHelperTest(
            string testName,
            ArchipelagoPacketBase firstPacket, ArchipelagoPacketBase secondPacket,
            Func<RoomStateHelper, T> getValue,
            T expectedValueAfterFirstPacket, T expectedValueAfterSecondPacket)
            : base(firstPacket, secondPacket, s => getValue(s), expectedValueAfterFirstPacket, expectedValueAfterSecondPacket)
        {
            SetName(testName);
        }
        public RoomStateHelperTest(
            string testName,
            ArchipelagoPacketBase firstPacket,
            Func<RoomStateHelper, T> getValue,
            T expectedValueAfterFirstUpdate)
            : this(testName, firstPacket, null, getValue, expectedValueAfterFirstUpdate, default)
        {
        }
    }
}
