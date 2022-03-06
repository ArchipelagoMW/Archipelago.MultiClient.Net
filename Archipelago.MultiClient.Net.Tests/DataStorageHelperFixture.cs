using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable All
// ReSharper disable UnusedVariable
namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class DataStorageHelperFixture
    {
        [Test]
        public void Should_throw_if_async_retrieval_timeout()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            Assert.Throws<TimeoutException>(() =>
            {
                int value = sut["KeyNope"];
            });
        }

        public static TestCaseData[] GetValueTests =>
            new TestCaseData[] {
                new GetValueTest<int>((sut, key) => sut[key], 10),
                new GetValueTest<long>((sut, key) => sut[key], 1000L),
                new GetValueTest<decimal>((sut, key) => sut[key], 1.001m),
                new GetValueTest<double>((sut, key) => sut[key], 1.01d),
                new GetValueTest<float>((sut, key) => sut[key], 10f),
                new GetValueTest<string>((sut, key) => sut[key], "text")
            };

        [TestCaseSource(nameof(GetValueTests))]
        public void Should_get_value_correctly<T>(Func<DataStorageHelper, string, T> getValue, T value)
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            var retrievalPacket = new RetrievedPacket
            {
                Data = new Dictionary<string, JToken> { { "Key", JToken.FromObject(value) } }
            };

            RaiseEventAsync(socket, retrievalPacket);

            T result = getValue(sut, "Key");

            Assert.That(result, Is.EqualTo(value));

            socket.DidNotReceive().SendPacket(Arg.Any<SetPacket>());
            socket.Received().SendPacketAsync(Arg.Is<GetPacket>(p => p.Keys[0] == "Key"));
        }

        public static TestCaseData[] CompoundAssignmentTests =>
            new TestCaseData[] {
                new CompoundAssignmentTest<int>("=", 30, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<long>("=", 300L, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<decimal>("=", 3.003m, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<double>("=", 3.03d, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<float>("=", 3f, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<string>("=", "test", (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (string)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<bool>("=", true, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (bool)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<List<int>>("=", new List<int>() { 1 , 3 }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<string[]>("=", new []{ "a", "b" }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<int>("+=", 10, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<long>("+=", 300L, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<decimal>("+=", 3.003m, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<double>("+=", 3.03d, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<float>("+=", 3f, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<string>("+=", "test", (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (string)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<IList>("+=", new []{ 1, 2 }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<int>>("+=", new List<int>() { 1 , 3 }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<string[]>("+=", new []{ "a", "b" }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<OperationSpecification>("+=", Bitwise.Xor(0xFF), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Xor && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<OperationSpecification>("+=", Bitwise.Or(0x00F0), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Or && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<OperationSpecification>("+=", Bitwise.And(0b01011), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.And && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<OperationSpecification>("+=", Bitwise.LeftShift(4), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.LeftShift && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<OperationSpecification>("+=", Bitwise.RightShift(8), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.RightShift && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<int>("-=", 10, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<long>("-=", 300L, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<decimal>("-=", 3.003m, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<double>("-=", 3.03d, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<float>("-=", 3f, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<int>("*=", 10, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<long>("*=", 300L, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<decimal>("*=", 3.003m, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<double>("*=", 3.03d, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<float>("*=", 3f, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<int>("/=", 10, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == 1m/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<long>("/=", 300L, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == 1m/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<decimal>("/=", 3.003m, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == 1m/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<double>("/=", 3.03d, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == 1d/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<float>("/=", 3f, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == 1d/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<int>("%=", 10, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<long>("%=", 300L, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<decimal>("%=", 3.003m, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<double>("%=", 3.03d, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<float>("%=", 3f, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<int>("^=", 10, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<long>("^=", 300L, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<decimal>("^=", 3.003m, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<double>("^=", 3.03d, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<float>("^=", 3f, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<int>("++", 0, (sut, key, value) => sut[key]++,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == 1 && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<int>("--", 0, (sut, key, value) => sut[key]--,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == -1 && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<int>("<<=", 10, (sut, key, value) => sut[key] <<= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Max),
                new CompoundAssignmentTest<int>(">>=", 10, (sut, key, value) => sut[key] >>= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Min)
            };

        [TestCaseSource(nameof(CompoundAssignmentTests))]
        public void Should_handle_compound_Assignment_correctly<T>(
            T value, Action<DataStorageHelper, string, T> action, 
            Func<SetPacket, string, T, bool> validatePacket)
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            action(sut, "Key", value);

            socket.DidNotReceive().SendPacket(Arg.Any<GetPacket>());
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(p => validatePacket(p, "Key", value) && p.Operations.Length == 1));
        }

        public static TestCaseData[] AssignmentTests =>
            new TestCaseData[] {
                new AssignmentTest<int>("+", 30, (sut, key) => sut[key] + 5, 35),
                new AssignmentTest<long>("+", 1000L, (sut, key) => sut[key] + 10L, 1010L),
                new AssignmentTest<decimal>("+", 1.001m, (sut, key) => sut[key] + 0.2m, 1.201m),
                new AssignmentTest<double>("+", 1.01d, (sut, key) => sut[key] + 0.01d, 1.02d),
                new AssignmentTest<float>("+", 10f, (sut, key) => sut[key] + 1f, 11f),
                new AssignmentTest<string>("+", "test", (sut, key) => sut[key] + "ing", "testing"),
                new AssignmentTest<int[]>("+", new []{ 1, 2 }, (sut, key) => ((JArray)(sut[key] + new []{ 3 })).ToObject<int[]>(), new []{ 1, 2, 3 }),
                new AssignmentTest<long>("+", 0xF0, (sut, key) => sut[key] + Bitwise.Xor(0xFF), 0x0F),
                new AssignmentTest<long>("+", 0xF0, (sut, key) => sut[key] + Bitwise.Or(0x0F), 0xFF),
                new AssignmentTest<long>("+", 0xFF, (sut, key) => sut[key] + Bitwise.And(0x0F), 0x0F),
                new AssignmentTest<long>("+", 0b101, (sut, key) => sut[key] + Bitwise.LeftShift(1), 0b1010),
                new AssignmentTest<long>("+", 0b101, (sut, key) => sut[key] + Bitwise.RightShift(2), 0b1),
                new AssignmentTest<int>("-", 30, (sut, key) => sut[key] - 5, 25),
                new AssignmentTest<long>("-", 1000L, (sut, key) => sut[key] - 10L, 990L),
                new AssignmentTest<decimal>("-", 1.001m, (sut, key) => sut[key] - 0.2m, 0.801m),
                new AssignmentTest<double>("-", 1.01d, (sut, key) => sut[key] - 0.01d, 1d),
                new AssignmentTest<float>("-", 10f, (sut, key) => sut[key] - 11f, -1f),
                new AssignmentTest<int>("*", 30, (sut, key) => sut[key] * 10, 300),
                new AssignmentTest<long>("*", 1000L, (sut, key) => sut[key] * 100L, 100000L),
                new AssignmentTest<decimal>("*", 1.001m, (sut, key) => sut[key] * 0.01m, 0.01001m),
                new AssignmentTest<double>("*", 1.01d, (sut, key) => sut[key] * 0.1d, 0.101d),
                new AssignmentTest<float>("*", 10f, (sut, key) => sut[key] * 2f, 20f),
                new AssignmentTest<int>("/", 30, (sut, key) => sut[key] / 10, 3),
                new AssignmentTest<long>("/", 1000L, (sut, key) => sut[key] / 100L, 10L),
                new AssignmentTest<decimal>("/", 1.001m, (sut, key) => sut[key] / 0.01m, 100.1m),
                new AssignmentTest<double>("/", 1.01d, (sut, key) => sut[key] / 0.1d, 10.1d),
                new AssignmentTest<float>("/", 10f, (sut, key) => sut[key] / 2f, 5f),
                new AssignmentTest<int>("%", 30, (sut, key) => sut[key] % 10, 0),
                new AssignmentTest<long>("%", 1000L, (sut, key) => sut[key] % 6L, 4L),
                new AssignmentTest<decimal>("%", 1.008m, (sut, key) => sut[key] % 0.3m, 0.108m),
                new AssignmentTest<double>("%", 1.01d, (sut, key) => sut[key] % 0.1d, 0.01d),
                new AssignmentTest<float>("%", 10f, (sut, key) => sut[key] % 3f, 1f),
                new AssignmentTest<int>("^", 30, (sut, key) => sut[key] ^ 2, 900),
                new AssignmentTest<long>("^", 3L, (sut, key) => sut[key] ^ 30L, 205891132094649L),
                new AssignmentTest<decimal>("^", 1.008m, (sut, key) => sut[key] ^ 0.3m, 1.00239331030046m), //Round off due to using double internally
                new AssignmentTest<double>("^", 3d, (sut, key) => sut[key] ^ 5d, 243d),
                new AssignmentTest<float>("^", 10f, (sut, key) => sut[key] ^ 3f, 1000f),
                new AssignmentTest<int>("<< 2", 2, (sut, key) => sut[key] << 5, 5),
                new AssignmentTest<int>("<< 20", 20, (sut, key) => sut[key] << 5, 20),
                new AssignmentTest<int>(">> 2", 2, (sut, key) => sut[key] >> 5, 2),
                new AssignmentTest<int>(">> 20", 20, (sut, key) => sut[key] >> 5, 5),
            };

        [TestCaseSource(nameof(AssignmentTests))]
        public void Should_handle_Assignment_correctly<T>(T baseValue, Func<DataStorageHelper, string, T> action, T expectedValue)
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            var retrievalPacket = new RetrievedPacket
            {
                Data = new Dictionary<string, JToken> { { "Key", JToken.FromObject(baseValue) } }
            };

            RaiseEventAsync(socket, retrievalPacket);

            T result = action(sut, "Key");

            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Should_throw_on_invalid_operation_on_string()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            var retrievalPacket = new RetrievedPacket
            {
                Data = new Dictionary<string, JToken> {
                    { "StringValueA", "A" },
                    { "StringValueB", "B" },
                    { "StringValueC", "C" },
                    { "StringValueD", "D" },
                }
            };

            RaiseEventAsync(socket, retrievalPacket);

            Assert.Throws<InvalidOperationException>(() => {
                string value = sut["StringValueA"] * 1.5d;
            });
            Assert.Throws<InvalidOperationException>(() => {
                string value = sut["StringValueB"] << 10;
            });
            Assert.Throws<InvalidOperationException>(() => {
                string value = sut["StringValueC"] >> 10;
            });
            Assert.Throws<InvalidOperationException>(() => {
                string value = sut["StringValueD"] / 2;
            });
        }

        [Test]
        public void Should_register_handler_and_recieve_update()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            object oldValueA = null;
            object newValueA = null;
            object newValueB = null;
            object newValueC = null;

            sut["Key"].OnValueChanged += (o, n) =>
            {
                oldValueA = (int)o;
                newValueA = (int)n;
            };
            sut["KeyB"].OnValueChanged += (o, n) =>
            {
                newValueB = (string)n;
            };
            sut["KeyB"].OnValueChanged += (o, n) =>
            {
                newValueC = (string)n;
            };

            socket.DidNotReceive().SendPacket(Arg.Any<GetPacket>());
            socket.DidNotReceive().SendPacket(Arg.Any<SetPacket>());
            socket.Received().SendPacketAsync(Arg.Is<SetNotifyPacket>(p => p.Keys[0] == "Key"));

            var setReplyPacketA = new SetReplyPacket { Key = "Key", OriginalValue = 10, Value = 20 };
            var setReplyPacketB = new SetReplyPacket { Key = "KeyB", OriginalValue = "Ola", Value = "Yeeh" };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(setReplyPacketA);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(setReplyPacketB);

            Assert.That(oldValueA, Is.EqualTo(10));
            Assert.That(newValueA, Is.EqualTo(20));
            Assert.That(newValueB, Is.EqualTo("Yeeh"));
            Assert.That(newValueC, Is.EqualTo("Yeeh"));
        }

        [Test]
        public void Should_unregister_handler()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            int callbackCount = 0;

            void OnValueChanged(object oldValue, object newValue)
            {
                callbackCount++;
            }

            sut["Key"].OnValueChanged += OnValueChanged;

            var setReplyPacketA = new SetReplyPacket { Key = "Key", OriginalValue = 10, Value = 20 };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(setReplyPacketA);

            sut["Key"].OnValueChanged -= OnValueChanged;

            var setReplyPacketB = new SetReplyPacket { Key = "Key", OriginalValue = 20, Value = 30 };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(setReplyPacketB);

            Assert.That(callbackCount, Is.EqualTo(1));
        }

        [Test]
        public void Should_retreive_values_async()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            object a = null;
            object b = null;
            object c = null;
            object d = null;

            sut.GetAsync<int>("Key", v =>
            {
                a = v;
            });
            sut.GetAsync<int>(Scope.Global, "Key", v =>
            {
                b = v;
            });

            socket.Received(2).SendPacketAsync(Arg.Is<GetPacket>(p => p.Keys[0] == "Key"));

            var retrievedPacketA = new RetrievedPacket { Data = new Dictionary<string, JToken> { { "Key", 10 } } };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(retrievedPacketA);

            sut.GetAsync("Key", v =>
            {
                c = (int)v;
            });
            sut.GetAsync(Scope.Global, "OtherKey", v =>
            {
                d = (string)v;
            });

            var retrievedPacketB = new RetrievedPacket { Data = new Dictionary<string, JToken> {
                { "Key", 20 },
                { "OtherKey", "yolo" },
            } };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(retrievedPacketB);

            socket.Received(3).SendPacketAsync(Arg.Is<GetPacket>(p => p.Keys[0] == "Key"));
            socket.Received().SendPacketAsync(Arg.Is<GetPacket>(p => p.Keys[0] == "OtherKey"));

            Assert.That(a, Is.EqualTo(10));
            Assert.That(b, Is.EqualTo(10));
            Assert.That(c, Is.EqualTo(20));
            Assert.That(d, Is.EqualTo("yolo"));
        }

        [Test]
        public void Different_scopes_should_not_interfere()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            ArchipelagoSession.Game = "UnitTest";

            var sut = new DataStorageHelper(socket);

            var connectedPacket = new ConnectedPacket
            {
                Slot = 12,
                Team = 5
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(connectedPacket);

            sut[Scope.Global, "A"] = "global";
            sut[Scope.Game, "A"] = "game";
            sut[Scope.Team, "A"] = "team";
            sut[Scope.Slot, "A"] = "slot";

            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "A" && p.Operations[0].Operation == Operation.Replace && (string)p.Operations[0].Value == "global"));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "Game:UnitTest:A" && p.Operations[0].Operation == Operation.Replace && (string)p.Operations[0].Value == "game"));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "Team:5:A" && p.Operations[0].Operation == Operation.Replace && (string)p.Operations[0].Value == "team"));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "Slot:12:A" && p.Operations[0].Operation == Operation.Replace && (string)p.Operations[0].Value == "slot"));

            var retrievedPacketA = new RetrievedPacket { Data = new Dictionary<string, JToken> {
                { "A", "global" },
                { "Game:UnitTest:A", "game"},
                { "Team:5:A", "team"},
                { "Slot:12:A", "slot"},
            }};

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(retrievedPacketA);

            string global = sut[Scope.Global, "A"];
            string game = sut[Scope.Game, "A"];
            string team = sut[Scope.Team, "A"];
            string slot = sut[Scope.Slot, "A"];

            Assert.That(global, Is.EqualTo("global"));
            Assert.That(game, Is.EqualTo("game"));
            Assert.That(team, Is.EqualTo("team"));
            Assert.That(slot, Is.EqualTo("slot"));
        }

        [Test]
        public void Should_correctly_handle_deplete()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            decimal actualDepleteValue1 = 0;
            decimal actualDepleteValue2 = 0;

            Guid callback1Reference = Guid.Empty;
            Guid callback2Reference = Guid.Empty;

            socket.SendPacketAsync(Arg.Do<SetPacket>(p => {
                if (p.Key == "DP1")
                    callback1Reference = p.Reference.Value;
                if (p.Key == "DP2")
                    callback2Reference = p.Reference.Value;
            }));

            sut.Deplete(Scope.Global, "DP1", 50, (value, newValue) =>
            {
                actualDepleteValue1 = newValue - value;
            });
            sut.Deplete("DP2", -11.53m, (value, newValue) =>
            {
                actualDepleteValue2 = newValue - value;
            });

            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "DP1" && p.WantReply == true && p.Operations.Length == 2
                    && p.Operations[0].Operation == Operation.Add && (decimal)p.Operations[0].Value == 50m
                    && p.Operations[1].Operation == Operation.Max && (int)p.Operations[1].Value == 0));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "DP2" && p.WantReply == true && p.Operations.Length == 2
                     && p.Operations[0].Operation == Operation.Add && (decimal)p.Operations[0].Value == -11.53m
                     && p.Operations[1].Operation == Operation.Max && (int)p.Operations[1].Value == 0));

            var setReplyPacketA = new SetReplyPacket()
            {
                Key = "DP1",
                Value = 50m,
                OriginalValue = 70m
            };
            var setReplyPacketB = new SetReplyPacket()
            {
                Key = "DP1",
                Value = 120m,
                OriginalValue = 70m,
                Reference = callback1Reference
            };
            var setReplyPacketC = new SetReplyPacket()
            {
                Key = "DP2",
                Value = 0m,
                OriginalValue = 10.36m,
                Reference = callback2Reference
            };
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(setReplyPacketA);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(setReplyPacketB);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(setReplyPacketC);

            Assert.That(actualDepleteValue1, Is.EqualTo(50m));
            Assert.That(actualDepleteValue2, Is.EqualTo(-10.36m));
        }

        [Test]
        public void Should_handle_complex_opperations()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            var retrievalPacket = new RetrievedPacket
            {
                Data = new Dictionary<string, JToken> { { "X", 100 } }
            };

            RaiseEventAsync(socket, retrievalPacket);

            sut["A"] = sut["A"] - 10 << 0;
            sut["B"] = (((sut["B"] + 5) * 8) / 2) - 3;
            sut["C"] = (((sut["X"] + Bitwise.And(0xFF)) * 8) / 2) - 3;

            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "A" && p.Operations.Length == 2
                    && p.Operations[0].Operation == Operation.Add && (int)p.Operations[0].Value == -10
                    && p.Operations[1].Operation == Operation.Max && (int)p.Operations[1].Value == 0));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "B" && p.Operations.Length == 4
                    && p.Operations[0].Operation == Operation.Add && (int)p.Operations[0].Value == 5
                    && p.Operations[1].Operation == Operation.Mul && (int)p.Operations[1].Value == 8
                    && p.Operations[2].Operation == Operation.Mul && (decimal)p.Operations[2].Value == 0.5m
                    && p.Operations[3].Operation == Operation.Add && (int)p.Operations[3].Value == -3));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "C" && p.Operations.Length == 5
                    && p.Operations[0].Operation == Operation.Replace && (int)p.Operations[0].Value == 100
                    && p.Operations[1].Operation == Operation.And && (int)p.Operations[1].Value == 0xFF
                    && p.Operations[2].Operation == Operation.Mul && (int)p.Operations[2].Value == 8
                    && p.Operations[3].Operation == Operation.Mul && (decimal)p.Operations[3].Value == 0.5m
                    && p.Operations[4].Operation == Operation.Add && (int)p.Operations[4].Value == -3));
        }

        [Test]
        public void Should_send_custom_objects()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            sut["Item"] = JObject.FromObject(new NetworkItem { Item = 1337, Location = 999, Player = 2, Flags = ItemFlags.Trap });
            sut["Anonymous"] = JObject.FromObject(new { A = 10, B = "Hello" });

            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "Item" && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JObject));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "Anonymous" && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JObject));
        }

        [Test]
        public void Should_recieve_custom_objects()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            var retrievalPacket = new RetrievedPacket
            {
                Data = new Dictionary<string, JToken> {
                    { "Item", JObject.FromObject(new NetworkItem {
                        Item = 1337, Location = 999, Player = 2, Flags = ItemFlags.Trap }
                    )},
                    { "Anonymous", JObject.FromObject(
                        new { A = 10, B = "Hello" }
                    )}
                }
            };

            RaiseEventAsync(socket, retrievalPacket);

            var networkItem = sut["Item"].To<NetworkItem>();
            var anonymousType = sut["Anonymous"].To<JObject>();

            Assert.That(networkItem.Item, Is.EqualTo(1337));
            Assert.That(networkItem.Location, Is.EqualTo(999));
            Assert.That(networkItem.Player, Is.EqualTo(2));
            Assert.That(networkItem.Flags, Is.EqualTo(ItemFlags.Trap));
            Assert.That((int)anonymousType["A"], Is.EqualTo(10));
            Assert.That((string)anonymousType["B"], Is.EqualTo("Hello"));
        }

        [Test]
        public void Should_set_default()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            sut.Initialize("A", 20);
            sut.Initialize(Scope.Global, "B", new string[0]);
            sut.Initialize("C", new List<long>(3) { 1L, 3L, 9L });
            sut.Initialize(Scope.Global, "D", "");

            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "A" && p.Operations[0].Operation == Operation.Default && (int)p.DefaultValue == 20));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "B" && p.Operations[0].Operation == Operation.Default && p.DefaultValue is JArray));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "C" && p.Operations[0].Operation == Operation.Default && p.DefaultValue is JArray));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "D" && p.Operations[0].Operation == Operation.Default && (string)p.DefaultValue == ""));
        }

        [Test]
        public void Should_not_throw_when_handling_async_callbacks_and_new_data_is_received()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();

            var sut = new DataStorageHelper(socket);

            var retrievedPacked = new RetrievedPacket() {
                Data = new Dictionary<string, JToken> { { "Key", 10 } }
            };

            var addNewAsyncCallback = new Task(() =>
            {
                Thread.Sleep(1);
                sut.GetAsync("Key", t => {});
            });

            sut.GetAsync("Key", t => Thread.Sleep(1));
            sut.GetAsync("Key", t => Thread.Sleep(1));
            sut.GetAsync("Key", t => Thread.Sleep(1));

            addNewAsyncCallback.Start();

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(retrievedPacked);

            Assert.DoesNotThrow(() =>
            {
                Task.WaitAll(addNewAsyncCallback);
            });
        }

        private static void RaiseEventAsync(IArchipelagoSocketHelper socket, ArchipelagoPacketBase packet)
        {
            Task.Factory.StartNew(() => {
                socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(packet);
            });
        }

        class GetValueTest<T> : TestCaseData
        {
            public GetValueTest(Func<DataStorageHelper, string, T> getValue, T value)
                : base(getValue, value)
            {
                TestName = $"{value} ({typeof(T)})";
            }
        }

        class CompoundAssignmentTest<T> : TestCaseData
        {
            public CompoundAssignmentTest(string type, T value, Action<DataStorageHelper, string, T> action,
                Func<SetPacket, string, T, bool> validatePacket)
                : base(value, action, validatePacket)
            {
                TestName = $"{type} {value} ({typeof(T)})";
            }
        }

        class AssignmentTest<T> : TestCaseData
        {
            public AssignmentTest(string type, T baseValue, Func<DataStorageHelper, string, T> action, T expectedValue)
                : base(baseValue, action, expectedValue)
            {
                TestName = $"{type} {baseValue} ({typeof(T)})";
            }
        }
    }
}
