using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Helpers;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Linq;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class DeathLinkFixture
    {
        [TestCase(@"[{
            ""cmd"": ""Bounced"",
            ""tags"": [""DeathLink""],
            ""data"": {
                ""time"": 1647617402.0973,
                ""source"": ""Jarno""
            }  
        }]", TestName = "Deathlink with float time")]
        [TestCase(@"[{
            ""cmd"": ""Bounced"",
            ""tags"": [""DeathLink""],
            ""data"": {
                ""time"": 1638709805.0738149,
                ""source"": ""Someone""
            }  
        }]", TestName = "Deathlink with bigger float time")]
        [TestCase(@"[{
            ""cmd"": ""Bounced"",
            ""tags"": [""DeathLink""],
            ""data"": {
                ""time"": 1647622071990,
                ""source"": ""Hello World""
            }  
        }]", TestName = "Deathlink with 64 bit unix-timestamp time")]
        [TestCase(@"[{
            ""cmd"": ""Bounced"",
            ""tags"": [""DeathLink""],
            ""data"": {
                ""time"": 1647617402.0973,
                ""source"": ""Bezerker"",
                ""cause"": ""As always""
            }  
        }]", TestName = "Deathlink with cause")]
        [TestCase(@"[{
            ""cmd"": ""Bounced"",
            ""tags"": [""DeathLink""],
            ""data"": {
                ""time"": 1647617402,
                ""source"": ""Jarno""
            }  
        }]", TestName = "Deathlink with without milliseconds")]
        public void Should_create_death_link(string json)
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DeathLinkService(socket, connectionInfo, new DataStorageHelper(socket, connectionInfo));

            DeathLink receivedDeathLink = null;

            sut.OnDeathLinkReceived += dl =>
            {
                receivedDeathLink = dl;
            };

            // ReSharper disable once AssignNullToNotNullAttribute
            var packet = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(json, new ArchipelagoPacketConverter()).First();

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(packet);

            Assert.That(receivedDeathLink, Is.Not.Null);
        }

        public static TestCaseData[] DeathLinkParseTests =>
            new [] {
                new TestCaseData(new Dictionary<string, JToken> {
                    { "time", new DateTime(2022, 2, 2, 22, 2, 22).ToUnixTimeStamp() },
                    { "source", "Test" },
                    { "cause", "Died" }
                }, new DeathLink("Test", "Died") {
                    Timestamp = new DateTime(2022, 2, 2, 22, 2, 22)
                }) { TestName = "Simple deathlink"},
                new TestCaseData(new Dictionary<string, JToken> {
                    { "time", new DateTime(2022, 2, 2, 22, 2, 22).ToUnixTimeStamp() },
                    { "source", "No_Cause" }
                }, new DeathLink("No_Cause") {
                    Timestamp = new DateTime(2022, 2, 2, 22, 2, 22)
                }) { TestName = "No Cause"}
            };

        [TestCaseSource(nameof(DeathLinkParseTests))]
        public void Should_parse_death_link(Dictionary<string, JToken> data, DeathLink expectedDeathLink)
        {
            Assert.IsTrue(DeathLink.TryParse(data, out var deathLink));

            Assert.That(deathLink.Timestamp, Is.EqualTo(expectedDeathLink.Timestamp));
            Assert.That(deathLink.Source, Is.EqualTo(expectedDeathLink.Source));
            Assert.That(deathLink.Cause, Is.EqualTo(expectedDeathLink.Cause));
        }
    }
}
