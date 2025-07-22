using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.MultiClient.Net.Tests
{

		[Test]
		public void Should_parse_room_info_packet()
		{
			const string message =
				@"[{""cmd"":""RoomInfo"",""password"":false,""players"":[],""games"":[""BatBoy""],""tags"":[""AP""],""version"":{ ""major"":0,""minor"":3,""build"":5,""class"":""Version""},""permissions"":{ ""forfeit"":2,""remaining"":2,""collect"":2},""hint_cost"":10,""location_check_points"":1,""datapackage_version"":108,""datapackage_versions"":{ ""Archipelago"":1,""A Link to the Past"":8,""ArchipIDLE"":4,""BatBoy"":2,""Sudoku"":1,""ChecksFinder"":4,""Dark Souls III"":2,""Donkey Kong Country 3"":2,""Factorio"":5,""Final Fantasy"":2,""Hollow Knight"":2,""Hylics 2"":1,""Meritous"":2,""Minecraft"":7,""Ocarina of Time"":2,""Ori and the Blind Forest"":1,""Overcooked! 2"":2,""Pokemon Red and Blue"":1,""Raft"":2,""Rogue Legacy"":3,""Risk of Rain 2"":4,""Sonic Adventure 2 Battle"":2,""Starcraft 2 Wings of Liberty"":3,""Super Metroid"":2,""Super Mario 64"":8,""Super Mario World"":1,""SMZ3"":3,""Secret of Evermore"":3,""Slay the Spire"":1,""Subnautica"":7,""Timespinner"":10,""VVVVVV"":1,""The Witness"":8,""Zillion"":1},""seed_name"":""48876073281086942113"",""time"":1667588510.9488218}]";

			var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(message, Converter);

			Assert.That(packets, Is.Not.Null);
			Assert.That(packets.Count, Is.EqualTo(1));

			var roomInfoPacket = packets[0] as RoomInfoPacket;

			Assert.That(roomInfoPacket, Is.Not.Null);

			Assert.That(roomInfoPacket.Version.Major, Is.EqualTo(0));
			Assert.That(roomInfoPacket.Version.Minor, Is.EqualTo(3));
			Assert.That(roomInfoPacket.Version.Build, Is.EqualTo(5));
			Assert.That(roomInfoPacket.Games.Length, Is.EqualTo(1));
			Assert.That(roomInfoPacket.HintCostPercentage, Is.EqualTo(10));
			Assert.That(roomInfoPacket.LocationCheckPoints, Is.EqualTo(1));
			Assert.That(roomInfoPacket.Password, Is.EqualTo(false));
			Assert.That(roomInfoPacket.SeedName, Is.EqualTo("48876073281086942113"));
		}

		[Test]
		public void Should_not_throw_on_receiving_unknown_packet()
		{
			const string message = @"[{""cmd"":""NewCommand"",""testing"":5,""something"":true}]";

			List<ArchipelagoPacketBase> packets = null;

			Assert.DoesNotThrow(() =>
			{
				packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(message, Converter);
			});

			Assert.That(packets, Is.Not.Null);
			Assert.That(packets.Count, Is.EqualTo(1));

			var unknownPacket = packets[0] as UnknownPacket;

			Assert.That(unknownPacket, Is.Not.Null);
			Assert.That(unknownPacket.ToJObject()["testing"].ToObject<int>(), Is.EqualTo(5));
			Assert.That(unknownPacket.ToJObject()["something"].ToObject<bool>(), Is.EqualTo(true));
		}

		[Test]
		public void Should_handle_multiple_packets_in_same_message()
		{
			const string message =
				@"[{""cmd"":""Retrieved"",""keys"":{""A"":10,""B"":6}},{""cmd"":""LocationInfo"",""locations"":[{""item"":1,""location"":2344,""player"":3,""flags"":0}]}]";

			var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(message, Converter);

			Assert.That(packets, Is.Not.Null);
			Assert.That(packets.Count, Is.EqualTo(2));

			Assert.That(packets.OfType<RetrievedPacket>().Count(), Is.EqualTo(1));
			Assert.That(packets.OfType<LocationInfoPacket>().Count(), Is.EqualTo(1));
		}

		[Test]
		public void Should_correctly_parse_unknown_enum_for_print_json()
		{
			// message part type "region_id"
			// print json type "NewType"
			// hintstatus 9
			const string message =
				@"[{""cmd"":""PrintJSON"",""data"":[{""text"":""[Hint]: ""},{""text"":""86"",""type"":""region_id""},{""text"":""'s ""},{""text"":""1337049"",""player"":2,""flags"":0,""type"":""item_id""},{""text"":"" is at ""},{""text"":""1337010"",""player"":2,""type"":""location_id""},{""text"":"" in ""},{""text"":""2"",""type"":""player_id""},{""text"":""'s World""},{""text"":"". ""},{""text"":""NEW FUTURE STATUS"",""hint_status"":9,""type"":""hint_status""}],""type"":""NewType"",""receiving"":2,""item"":{""item"":1337049,""location"":1337010,""player"":2,""flags"":0,""class"":""NetworkItem""},""found"":false}]";

			var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(message, Converter);

			Assert.That(packets, Is.Not.Null);
			Assert.That(packets.Count, Is.EqualTo(1));

			var printJson = packets[0] as PrintJsonPacket;

			Assert.That(printJson, Is.Not.Null);
			Assert.That(printJson.MessageType, Is.Null); // "NewType" cannot be parsed

			Assert.That(printJson.Data[0].Type , Is.Null);
			Assert.That(printJson.Data[1].Type , Is.Null); //region_id cannot be parsed
			Assert.That(printJson.Data[2].Type , Is.Null);
			Assert.That(printJson.Data[3].Type , Is.EqualTo(JsonMessagePartType.ItemId));
			Assert.That(printJson.Data[4].Type , Is.Null);
			Assert.That(printJson.Data[5].Type , Is.EqualTo(JsonMessagePartType.LocationId));
			Assert.That(printJson.Data[6].Type , Is.Null);
			Assert.That(printJson.Data[7].Type , Is.EqualTo(JsonMessagePartType.PlayerId));
			Assert.That(printJson.Data[8].Type , Is.Null);
			Assert.That(printJson.Data[9].Type , Is.Null);
			Assert.That(printJson.Data[10].HintStatus, Is.EqualTo((HintStatus)9)); // 9 is not a valid hint status
		}

		[Test]
		public void Should_correctly_parse_unknown_connection_refused()
		{
			// errors "NewErrorType"
			const string message = @"[{""cmd"":""ConnectionRefused"",""errors"":[""InvalidSlot"", ""NewErrorType""]}]";

			var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(message, Converter);

			Assert.That(packets, Is.Not.Null);
			Assert.That(packets.Count, Is.EqualTo(1));

			var connectionRefusedPacket = packets[0] as ConnectionRefusedPacket;

			Assert.That(connectionRefusedPacket, Is.Not.Null);

			Assert.That(connectionRefusedPacket.Errors.Length, Is.EqualTo(2));

			Assert.That(connectionRefusedPacket.Errors[0], Is.EqualTo(ConnectionRefusedError.InvalidSlot));
			Assert.That(connectionRefusedPacket.Errors[1], Is.EqualTo(ConnectionRefusedError.UnknownError));
		}

		[Test]
		public void Should_correctly_parse_unknown_data_storage_opperations()
		{
			// opperation type "something_specail"
			const string message = @"[{""cmd"":""SetReply"",""key"":""GiftBox;0;2"",""default"":{},""operations"":[{""operation"":""default"",""value"":null},{""operation"":""something_specail"",""value"":null},{""operation"":""left_shift"",""value"":1}],""want_reply"":false,""original_value"":{},""slot"":2,""value"":{}}]";

			var packets = JsonConvert.DeserializeObject<List<ArchipelagoPacketBase>>(message, Converter);

			Assert.That(packets, Is.Not.Null);
			Assert.That(packets.Count, Is.EqualTo(1));

			var connectionRefusedPacket = packets[0] as SetReplyPacket;

			Assert.That(connectionRefusedPacket, Is.Not.Null);

			Assert.That(connectionRefusedPacket.Operations.Length, Is.EqualTo(3));

			Assert.That(connectionRefusedPacket.Operations[0].OperationType, Is.EqualTo(OperationType.Default));
			Assert.That(connectionRefusedPacket.Operations[1].OperationType, Is.EqualTo(OperationType.Unknown));
			Assert.That(connectionRefusedPacket.Operations[2].OperationType, Is.EqualTo(OperationType.LeftShift));
		}
	}
}
