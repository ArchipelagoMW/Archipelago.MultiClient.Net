﻿using Archipelago.MultiClient.Net.Enums;
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
using Callback = Archipelago.MultiClient.Net.Models.Callback;

// ReSharper disable All
// ReSharper disable UnusedVariable
namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class DataStorageHelperFixture
    {
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
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            T result = default;

            ExecuteAsyncWithDelay(
                () => result = getValue(sut, "Key"),
                () => RaiseRetrieved(socket, "Key", JToken.FromObject(value)));

            Assert.That(result, Is.EqualTo(value));

            socket.DidNotReceive().SendPacket(Arg.Any<SetPacket>());
            socket.Received().SendPacketAsync(Arg.Is<GetPacket>(p => p.Keys[0] == "Key"));
        }

        public static TestCaseData[] CompoundAssignmentTests =>
            new TestCaseData[] {
                new CompoundAssignmentTest<int>("Assignment", 30, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<long>("Assignment", 300L, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<decimal>("Assignment", 3.003m, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<double>("Assignment", 3.03d, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<float>("Assignment", 3f, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<string>("Assignment", "test", (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (string)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<bool>("Assignment", true, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && (bool)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Replace),
                new CompoundAssignmentTest<bool[]>("Assignment", new []{ true, false }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<int[]>("Assignment", new []{ 101, 204 }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<long[]>("Assignment", new []{ 500L, 999L }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<decimal[]>("Assignment", new []{ 1m, 0.5m }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<double[]>("Assignment", new []{ 77d, 0.23d }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<float[]>("Assignment", new []{ 1f }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<string[]>("Assignment", new []{ "a", "b" }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<object[]>("Assignment", new object[]{ "A", 20 }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<ItemFlags[]>("Assignment", new []{ ItemFlags.Trap }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<bool>>("Assignment", new List<bool>() { true }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<int>>("Assignment", new List<int>() { 1 , 3 }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<long>>("Assignment", new List<long>() { 1L , 2L }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<decimal>>("Assignment", new List<decimal>() { 1.01m }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<double>>("Assignment", new List<double>() { 1.3d , 3.1d }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<float>>("Assignment", new List<float>() { 5f }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<string>>("Assignment", new List<string>() { "Hello", "Kitty" }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<object>>("Assignment", new List<object>() { "Hello", 101 }, (sut, key, value) => sut[key] = value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<int>("Inplace Addition", 10, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<long>("Inplace Addition", 300L, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<decimal>("Inplace Addition", 3.003m, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<double>("Inplace Addition", 3.03d, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<float>("Inplace Addition", 3f, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<string>("Inplace Addition", "test", (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && (string)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<IList>("Inplace Addition", new []{ 1, 2 }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<bool[]>("Inplace Addition", new []{ true, false }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<int[]>("Inplace Addition", new []{ 101, 204 }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<long[]>("Inplace Addition", new []{ 500L, 999L }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<decimal[]>("Inplace Addition", new []{ 1m, 0.5m }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<double[]>("Inplace Addition", new []{ 77d, 0.23d }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<float[]>("Inplace Addition", new []{ 1f }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<string[]>("Inplace Addition", new []{ "a", "b" }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<object[]>("Inplace Addition", new object[]{ "A", 20 }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<ItemFlags[]>("Inplace Addition", new []{ ItemFlags.Trap }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<bool>>("Inplace Addition", new List<bool>() { true }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<int>>("Inplace Addition", new List<int>() { 1 , 3 }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<long>>("Inplace Addition", new List<long>() { 1L , 2L }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<decimal>>("Inplace Addition", new List<decimal>() { 1.01m }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<double>>("Inplace Addition", new List<double>() { 1.3d , 3.1d }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<float>>("Inplace Addition", new List<float>() { 5f }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<string>>("Inplace Addition", new List<string>() { "Hello", "Kitty" }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<List<object>>("Inplace Addition", new List<object>() { "Hello", 101 }, (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Add && p.Operations[0].Value is JArray),
                new CompoundAssignmentTest<OperationSpecification>("Inplace Addition", Bitwise.Xor(0xFF), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Xor && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<OperationSpecification>("Inplace Addition", Bitwise.Or(0x00F0), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.Or && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<OperationSpecification>("Inplace Addition", Bitwise.And(0b01011), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.And && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<OperationSpecification>("Inplace Addition", Bitwise.LeftShift(4), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.LeftShift && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<OperationSpecification>("Inplace Addition", Bitwise.RightShift(8), (sut, key, value) => sut[key] += value,
                    (p, key, value) => p.Key == key && p.Operations[0].Operation == Operation.RightShift && p.Operations[0].Value == value.Value),
                new CompoundAssignmentTest<int>("Inplace Subtraction", 10, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<long>("Inplace Subtraction", 300L, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<decimal>("Inplace Subtraction", 3.003m, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<double>("Inplace Subtraction", 3.03d, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<float>("Inplace Subtraction", 3f, (sut, key, value) => sut[key] -= value,
                    (p, key, value) => p.Key == key && -(float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<int>("Inplace Multiplication", 10, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<long>("Inplace Multiplication", 300L, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<decimal>("Inplace Multiplication", 3.003m, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<double>("Inplace Multiplication", 3.03d, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<float>("Inplace Multiplication", 3f, (sut, key, value) => sut[key] *= value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<int>("Inplace Division", 10, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == 1m/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<long>("Inplace Division", 300L, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == 1m/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<decimal>("Inplace Division", 3.003m, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == 1m/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<double>("Inplace Division", 3.03d, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == 1d/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<float>("Inplace Division", 3f, (sut, key, value) => sut[key] /= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == 1d/value && p.Operations[0].Operation == Operation.Mul),
                new CompoundAssignmentTest<int>("Inplace Modulus", 10, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<long>("Inplace Modulus", 300L, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<decimal>("Inplace Modulus", 3.003m, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<double>("Inplace Modulus", 3.03d, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<float>("Inplace Modulus", 3f, (sut, key, value) => sut[key] %= value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Mod),
                new CompoundAssignmentTest<int>("Inplace Exponentiation", 10, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<long>("Inplace Exponentiation", 300L, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (long)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<decimal>("Inplace Exponentiation", 3.003m, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (decimal)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<double>("Inplace Exponentiation", 3.03d, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (double)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<float>("Inplace Exponentiation", 3f, (sut, key, value) => sut[key] ^= value,
                    (p, key, value) => p.Key == key && (float)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Pow),
                new CompoundAssignmentTest<int>("Postfix Addition", 0, (sut, key, value) => sut[key]++,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == 1 && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<int>("Postfix Subtraction", 0, (sut, key, value) => sut[key]--,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == -1 && p.Operations[0].Operation == Operation.Add),
                new CompoundAssignmentTest<int>("Inplace Maximum", 10, (sut, key, value) => sut[key] <<= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Max),
                new CompoundAssignmentTest<int>("Inplace Minimum", 10, (sut, key, value) => sut[key] >>= value,
                    (p, key, value) => p.Key == key && (int)p.Operations[0].Value == value && p.Operations[0].Operation == Operation.Min)
            };

        [TestCaseSource(nameof(CompoundAssignmentTests))]
        public void Should_handle_compound_Assignment_correctly<T>(
            T value, Action<DataStorageHelper, string, T> action, 
            Func<SetPacket, string, T, bool> validatePacket)
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            action(sut, "Key", value);

            socket.DidNotReceive().SendPacket(Arg.Any<GetPacket>());
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(p => validatePacket(p, "Key", value) && p.Operations.Length == 1));
        }

        public static TestCaseData[] AssignmentTests =>
            new TestCaseData[] {
                new AssignmentTest<int>("Addition", 30, (sut, key) => sut[key] + 5, 35),
                new AssignmentTest<long>("Addition", 1000L, (sut, key) => sut[key] + 10L, 1010L),
                new AssignmentTest<decimal>("Addition", 1.001m, (sut, key) => sut[key] + 0.2m, 1.201m),
                new AssignmentTest<double>("Addition", 1.01d, (sut, key) => sut[key] + 0.01d, 1.02d),
                new AssignmentTest<float>("Addition", 10f, (sut, key) => sut[key] + 1f, 11f),
                new AssignmentTest<string>("Addition", "test", (sut, key) => sut[key] + "ing", "testing"),
                new AssignmentTest<bool[]>("Addition", new []{ false }, (sut, key) => sut[key] + new []{ true }, new []{ false, true }),
                new AssignmentTest<int[]>("Addition", new []{ 1, 2 }, (sut, key) => sut[key] + new []{ 3 }, new []{ 1, 2, 3 }),
                new AssignmentTest<long[]>("Addition", new []{ 2L }, (sut, key) => sut[key] + new []{ 4L }, new []{ 2L, 4L }),
                new AssignmentTest<decimal[]>("Addition", new []{ 1.056m }, (sut, key) => sut[key] + new []{ 3 }, new []{ 1.056m, 3m }),
                new AssignmentTest<double[]>("Addition", new []{ 1.2d }, (sut, key) => sut[key] + new double[]{ }, new []{ 1.2d }),
                new AssignmentTest<float[]>("Addition", new []{ 1f }, (sut, key) => sut[key] + new []{ 9f }, new []{ 1f, 9f }),
                new AssignmentTest<string[]>("Addition", new []{ "test" }, (sut, key) => sut[key] + new []{ "ing" }, new []{ "test", "ing" }),
                new AssignmentTest<object[]>("Addition", new object[]{ "test", 2 }, (sut, key) => sut[key] + new object[]{ "ing" }, new object[]{ "test", 2, "ing" }),
                new AssignmentTest<List<bool>>("Addition", new List<bool>{ false }, (sut, key) => sut[key] + new List<bool>{ true }, new List<bool>{ false, true }),
                new AssignmentTest<List<int>>("Addition", new List<int>{ 1 }, (sut, key) => sut[key] + new List<int>{ 2 }, new List<int>{ 1, 2 }),
                new AssignmentTest<List<long>>("Addition", new List<long>{ 3L }, (sut, key) => sut[key] + new List<long>{ 5L }, new List<long>{ 3L, 5L }),
                new AssignmentTest<List<decimal>>("Addition", new List<decimal>{ 7m }, (sut, key) => sut[key] + new List<decimal>{ 1m }, new List<decimal>{ 7m, 1m }),
                new AssignmentTest<List<double>>("Addition", new List<double>{ 6d }, (sut, key) => sut[key] + new List<double>{ 6d }, new List<double>{ 6d, 6d }),
                new AssignmentTest<List<float>>("Addition", new List<float>{ 2f }, (sut, key) => sut[key] + new List<float>{ 3f }, new List<float>{ 2f, 3f }),
                new AssignmentTest<List<string>>("Addition", new List<string>{ "A" }, (sut, key) => sut[key] + new List<string>{ "B" }, new List<string>{ "A", "B" }),
                new AssignmentTest<List<object>>("Addition", new List<object>{ 1 }, (sut, key) => sut[key] + new List<object>{ "C" }, new List<object>{ 1, "C" }),
                new AssignmentTest<long>("Addition", 0xF0, (sut, key) => sut[key] + Bitwise.Xor(0xFF), 0x0F),
                new AssignmentTest<long>("Addition", 0xF0, (sut, key) => sut[key] + Bitwise.Or(0x0F), 0xFF),
                new AssignmentTest<long>("Addition", 0xFF, (sut, key) => sut[key] + Bitwise.And(0x0F), 0x0F),
                new AssignmentTest<long>("Addition", 0b101, (sut, key) => sut[key] + Bitwise.LeftShift(1), 0b1010),
                new AssignmentTest<long>("Addition", 0b101, (sut, key) => sut[key] + Bitwise.RightShift(2), 0b1),
                new AssignmentTest<int>("Subtraction", 30, (sut, key) => sut[key] - 5, 25),
                new AssignmentTest<long>("Subtraction", 1000L, (sut, key) => sut[key] - 10L, 990L),
                new AssignmentTest<decimal>("Subtraction", 1.001m, (sut, key) => sut[key] - 0.2m, 0.801m),
                new AssignmentTest<double>("Subtraction", 1.01d, (sut, key) => sut[key] - 0.01d, 1d),
                new AssignmentTest<float>("Subtraction", 10f, (sut, key) => sut[key] - 11f, -1f),
                new AssignmentTest<int>("Multiplication", 30, (sut, key) => sut[key] * 10, 300),
                new AssignmentTest<long>("Multiplication", 1000L, (sut, key) => sut[key] * 100L, 100000L),
                new AssignmentTest<decimal>("Multiplication", 1.001m, (sut, key) => sut[key] * 0.01m, 0.01001m),
                new AssignmentTest<double>("Multiplication", 1.01d, (sut, key) => sut[key] * 0.1d, 0.101d),
                new AssignmentTest<float>("Multiplication", 10f, (sut, key) => sut[key] * 2f, 20f),
                new AssignmentTest<int>("Division", 30, (sut, key) => sut[key] / 10, 3),
                new AssignmentTest<long>("Division", 1000L, (sut, key) => sut[key] / 100L, 10L),
                new AssignmentTest<decimal>("Division", 1.001m, (sut, key) => sut[key] / 0.01m, 100.1m),
                new AssignmentTest<double>("Division", 1.01d, (sut, key) => sut[key] / 0.1d, 10.1d),
                new AssignmentTest<float>("Division", 10f, (sut, key) => sut[key] / 2f, 5f),
                new AssignmentTest<int>("Modulus", 30, (sut, key) => sut[key] % 10, 0),
                new AssignmentTest<long>("Modulus", 1000L, (sut, key) => sut[key] % 6L, 4L),
                new AssignmentTest<decimal>("Modulus", 1.008m, (sut, key) => sut[key] % 0.3m, 0.108m),
                new AssignmentTest<double>("Modulus", 1.01d, (sut, key) => sut[key] % 0.1d, 0.01d),
                new AssignmentTest<float>("Modulus", 10f, (sut, key) => sut[key] % 3f, 1f),
                new AssignmentTest<int>("Exponentiation", 30, (sut, key) => sut[key] ^ 2, 900),
                new AssignmentTest<long>("Exponentiation", 3L, (sut, key) => sut[key] ^ 30L, 205891132094649L),
                new AssignmentTest<decimal>("Exponentiation", 1.008m, (sut, key) => sut[key] ^ 0.3m, 1.00239331030046m), //Round off due to using double internally
                new AssignmentTest<double>("Exponentiation", 3d, (sut, key) => sut[key] ^ 5d, 243d),
                new AssignmentTest<float>("Exponentiation", 10f, (sut, key) => sut[key] ^ 3f, 1000f),
                new AssignmentTest<int>("Maximum", 2, (sut, key) => sut[key] << 5, 5),
                new AssignmentTest<int>("Maximum", 20, (sut, key) => sut[key] << 5, 20),
                new AssignmentTest<int>("Minimum", 2, (sut, key) => sut[key] >> 5, 2),
                new AssignmentTest<int>("Minimum", 20, (sut, key) => sut[key] >> 5, 5),
            };

        [TestCaseSource(nameof(AssignmentTests))]
        public void Should_handle_Assignment_correctly<T>(T baseValue, Func<DataStorageHelper, string, T> action, T expectedValue)
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            T result = default;

            ExecuteAsyncWithDelay(
                () => result = action(sut, "Key"),
                () => RaiseRetrieved(socket, "Key", JToken.FromObject(baseValue)));

            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Should_throw_on_invalid_operation_on_string()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            string result;

            ExecuteAsyncWithDelay(
                () => Assert.Throws<InvalidOperationException>(() => { result = sut["StringValueA"] * 1.5d; }),
                () => { 
                    Thread.Sleep(100);
                    RaiseRetrieved(socket, "StringValueA", "A");
                });
            ExecuteAsyncWithDelay(
                () => Assert.Throws<InvalidOperationException>(() => { result = sut["StringValueB"] << 10; }),
                () => {
                    Thread.Sleep(100);
                    RaiseRetrieved(socket, "StringValueB", "B");
                });
            ExecuteAsyncWithDelay(
                () => Assert.Throws<InvalidOperationException>(() => { result = sut["StringValueC"] >> 10; }),
                () => {
                    Thread.Sleep(100);
                    RaiseRetrieved(socket, "StringValueC", "C");
                });
            ExecuteAsyncWithDelay(
                () => Assert.Throws<InvalidOperationException>(() => { result = sut["StringValueD"] / 2; }),
                () => {
                    Thread.Sleep(100);
                    RaiseRetrieved(socket, "StringValueD", "D");
                });
        }

        [Test]
        public void Should_register_handler_and_recieve_update()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

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
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

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
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            object a = null;
            object b = null;
            object c = null;
            object d = null;

            sut["Key"].GetAsync<int>(v =>
            {
                a = v;
            });
            sut[Scope.Global, "Key"].GetAsync<int>(v =>
            {
                b = v;
            });

            socket.Received(2).SendPacketAsync(Arg.Is<GetPacket>(p => p.Keys[0] == "Key"));

            var retrievedPacketA = new RetrievedPacket { Data = new Dictionary<string, JToken> { { "Key", 10 } } };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(retrievedPacketA);

            sut["Key"].GetAsync(v =>
            {
                c = (int)v;
            });
            sut[Scope.Global, "OtherKey"].GetAsync(v =>
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
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();
            connectionInfo.Game.Returns("UnitTest");
            connectionInfo.Team.Returns(5);
            connectionInfo.Slot.Returns(12);

            var sut = new DataStorageHelper(socket, connectionInfo);

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

            string global = default;
            string game = default;
            string team = default;
            string slot = default;

            ExecuteAsyncWithDelay(
                () => global = sut[Scope.Global, "A"],
                () => RaiseRetrieved(socket, "A", "global"));
            ExecuteAsyncWithDelay(
                () => game = sut[Scope.Game, "A"],
                () => RaiseRetrieved(socket, "Game:UnitTest:A", "game"));
            ExecuteAsyncWithDelay(
                () => team = sut[Scope.Team, "A"],
                () => RaiseRetrieved(socket, "Team:5:A", "team"));
            ExecuteAsyncWithDelay(
                () => slot = sut[Scope.Slot, "A"],
                () => RaiseRetrieved(socket, "Slot:12:A", "slot"));

            Assert.That(global, Is.EqualTo("global"));
            Assert.That(game, Is.EqualTo("game"));
            Assert.That(team, Is.EqualTo("team"));
            Assert.That(slot, Is.EqualTo("slot"));
        }

        [Test]
        public void Should_correctly_handle_deplete()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

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

            sut[Scope.Global, "DP1"] = ((sut[Scope.Global, "DP1"] + 50) << 0) + Callback.Add((o, n) =>
            {
                actualDepleteValue1 = (decimal)n - (decimal)o;
            });
            sut["DP2"] = ((sut["DP2"] - 11.53m) << 0) + Callback.Add((o, n) =>
            {
                actualDepleteValue2 = (decimal)n - (decimal)o;
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
                Value = 120,
                OriginalValue = 70,
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
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            sut["A"] = sut["A"] - 10 << 0;
            sut["B"] = (((sut["B"] + 5) * 8) / 2) - 3;

            ExecuteAsyncWithDelay(
                () => sut["C"] = (((sut["X"] + Bitwise.And(0xFF)) * 8) / 2) - 3,
                () => RaiseRetrieved(socket, "X", 100));

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
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            sut["Item"] = JObject.FromObject(new NetworkItem { Item = 1337, Location = 999, Player = 2, Flags = ItemFlags.Trap });
            sut["Anonymous"] = JObject.FromObject(new { A = 10, B = "Hello" });

            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "Item" && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JObject));
            socket.Received().SendPacketAsync(Arg.Is<SetPacket>(
                p => p.Key == "Anonymous" && p.Operations[0].Operation == Operation.Replace && p.Operations[0].Value is JObject));
        }

        [Test]
        public void Should_retrieve_custom_objects()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            NetworkItem networkItem = default;
            JObject anonymousType = default;

            ExecuteAsyncWithDelay(
                () => networkItem = sut["Item"].To<NetworkItem>(),
                () => RaiseRetrieved(socket, "Item", JObject.FromObject(new NetworkItem {
                    Item = 1337,
                    Location = 999,
                    Player = 2,
                    Flags = ItemFlags.Trap
                })));
            ExecuteAsyncWithDelay(
                () => anonymousType = sut["Anonymous"].To<JObject>(),
                () => RaiseRetrieved(socket, "Anonymous", JObject.FromObject(new { 
                    A = 10, 
                    B = "Hello"
                })));

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
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            sut["A"].Initialize(20);
            sut[Scope.Global, "B"].Initialize(new string[0]);
            sut["C"].Initialize(new List<long>(3) { 1L, 3L, 9L });
            sut[Scope.Global, "D"].Initialize("");

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
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            var retrievedPacked = new RetrievedPacket() {
                Data = new Dictionary<string, JToken> { { "Key", 10 } }
            };

            var addNewAsyncCallback = new Task(() =>
            {
                Thread.Sleep(1);
                sut["Key"].GetAsync(t => {});
            });

            sut["Key"].GetAsync(t => Thread.Sleep(1));
            sut["Key"].GetAsync(t => Thread.Sleep(1));
            sut["Key"].GetAsync(t => Thread.Sleep(1));

            addNewAsyncCallback.Start();

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(retrievedPacked);

            Assert.DoesNotThrow(() =>
            {
                Task.WaitAll(addNewAsyncCallback);
            });
        }

        [Test]
        public void Should_throw_on_invallid_scope()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                sut[(Scope)6, "Key"] = 10;
            });
        }

        [Test]
        public void Should_not_use_old_data_that_is_requested_outside_of_the_datastorage()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var connectionInfo = Substitute.For<IConnectionInfoProvider>();

            var sut = new DataStorageHelper(socket, connectionInfo);

            var oldRetrievedPacket = new RetrievedPacket()
            {
                Data = new Dictionary<string, JToken> { { "Key", 10 } }
            };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(oldRetrievedPacket);

            int result = default;

            ExecuteAsyncWithDelay(
                () => result = sut["Key"],
                () => RaiseRetrieved(socket, "Key", 25));

            Assert.That(result, Is.EqualTo(25));
        }

        public static void ExecuteAsyncWithDelay(Action retrieve, Action raiseEvent)
        {
            Task.WaitAll( new [] {
                Task.Factory.StartNew(() =>
                {
                    retrieve();
                }), 
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(100);
                    raiseEvent();
                })
            }, 3000);
        }

        public static void RaiseRetrieved(IArchipelagoSocketHelper socket, string key, JToken value)
        {
            var packet = new RetrievedPacket() { Data = new Dictionary<string, JToken> { { key, value } } };

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(packet);
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
