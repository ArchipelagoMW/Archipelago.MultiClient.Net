using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
                Data = new Dictionary<string, object> { { "Key", value } }
            };

            RaiseEventAsync(socket, retrievalPacket);

            T result = getValue(sut, "Key");

            Assert.That(result, Is.EqualTo(value));
        }

        public static TestCaseData[] CompoundAssignmentTests =>
            new TestCaseData[] {
                new CompoundAssignmentTest<int>("=", 30, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (int)p.Value == value && p.Operation == "replace"),
                new CompoundAssignmentTest<long>("=", 300L, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (long)p.Value == value && p.Operation == "replace"),
                new CompoundAssignmentTest<decimal>("=", 3.003m, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (decimal)p.Value == value && p.Operation == "replace"),
                new CompoundAssignmentTest<double>("=", 3.03d, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (double)p.Value == value && p.Operation == "replace"),
                new CompoundAssignmentTest<float>("=", 3f, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (float)p.Value == value && p.Operation == "replace"),
                new CompoundAssignmentTest<string>("=", "test", (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (string)p.Value == value && p.Operation == "replace"),
                new CompoundAssignmentTest<int>("+=", 10, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (int)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<long>("+=", 300L, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (long)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<decimal>("+=", 3.003m, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (decimal)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<double>("+=", 3.03d, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (double)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<float>("+=", 3f, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (float)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<string>("+=", "test", (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (string)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<int>("-=", 10, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(int)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<long>("-=", 300L, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(long)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<decimal>("-=", 3.003m, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(decimal)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<double>("-=", 3.03d, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(double)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<float>("-=", 3f, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(float)p.Value == value && p.Operation == "add"),
                new CompoundAssignmentTest<int>("*=", 10, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (int)p.Value == value && p.Operation == "mul"),
                new CompoundAssignmentTest<long>("*=", 300L, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (long)p.Value == value && p.Operation == "mul"),
                new CompoundAssignmentTest<decimal>("*=", 3.003m, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (decimal)p.Value == value && p.Operation == "mul"),
                new CompoundAssignmentTest<double>("*=", 3.03d, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (double)p.Value == value && p.Operation == "mul"),
                new CompoundAssignmentTest<float>("*=", 3f, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (float)p.Value == value && p.Operation == "mul"),
                new CompoundAssignmentTest<int>("/=", 10, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Value == 1m/value && p.Operation == "mul"),
                new CompoundAssignmentTest<long>("/=", 300L, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Value == 1m/value && p.Operation == "mul"),
                new CompoundAssignmentTest<decimal>("/=", 3.003m, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Value == 1m/value && p.Operation == "mul"),
                new CompoundAssignmentTest<double>("/=", 3.03d, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (double)p.Value == 1d/value && p.Operation == "mul"),
                new CompoundAssignmentTest<float>("/=", 3f, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (double)p.Value == 1d/value && p.Operation == "mul"),
                new CompoundAssignmentTest<int>("++", 0, (sut, key, value) => sut[key]++,
                    (p, key, value) => p.Key == key && (int)p.Value == 1 && p.Operation == "add"),
                new CompoundAssignmentTest<int>("--", 0, (sut, key, value) => sut[key]--,
                    (p, key, value) => p.Key == key && (int)p.Value == -1 && p.Operation == "add"),
                new CompoundAssignmentTest<int>("<<=", 10, (sut, key, value) => sut[key] <<= value,
                    (p, key, value) => p.Key == key && (int)p.Value == value && p.Operation == "max"),
                new CompoundAssignmentTest<int>(">>=", 10, (sut, key, value) => sut[key] >>= value,
                    (p, key, value) => p.Key == key && (int)p.Value == value && p.Operation == "min")
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
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(p => validatePacket(p, "Key", value)));
        }

        public static TestCaseData[] AssignmentTests =>
            new TestCaseData[] {
                new AssignmentTest<int>("+", 30, (sut, key) => sut[key] + 5, 35),
                new AssignmentTest<long>("+", 1000L, (sut, key) => sut[key] + 10L, 1010L),
                new AssignmentTest<decimal>("+", 1.001m, (sut, key) => sut[key] + 0.2m, 1.201m),
                new AssignmentTest<double>("+", 1.01d, (sut, key) => sut[key] + 0.01d, 1.02d),
                new AssignmentTest<float>("+", 10f, (sut, key) => sut[key] + 1f, 11f),
                new AssignmentTest<string>("+", "test", (sut, key) => sut[key] + "ing", "testing"),
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
                Data = new Dictionary<string, object> { { "Key", baseValue } }
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
                Data = new Dictionary<string, object> {
                    { "StringValueA", "A" },
                    { "StringValueB", "B" },
                    { "StringValueC", "C" },
                }
            };

            RaiseEventAsync(socket, retrievalPacket);

            Assert.Throws<InvalidOperationException>(() => {
                string value = sut["StringValueA"] *= 10;
            });
            Assert.Throws<InvalidOperationException>(() => {
                string value = sut["StringValueB"] <<= 10;
            });
            Assert.Throws<InvalidOperationException>(() => {
                string value = sut["StringValueC"] >>= 10;
            });
        }

        //TODO test retrieval async
        //TODO test deplete
        //TODO OnValueChanged 


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
                TestName = $"{typeof(T)}";
            }
        }

        class CompoundAssignmentTest<T> : TestCaseData
        {
            public CompoundAssignmentTest(string type, T value, Action<DataStorageHelper, string, T> action,
                Func<SetPacket, string, T, bool> validatePacket)
                : base(value, action, validatePacket)
            {
                TestName = $"{typeof(T)} {type}";
            }
        }

        class AssignmentTest<T> : TestCaseData
        {
            public AssignmentTest(string type, T baseValue, Func<DataStorageHelper, string, T> action, T expectedValue)
                : base(baseValue, action, expectedValue)
            {
                TestName = $"{typeof(T)} {type}";
            }
        }
    }
}
