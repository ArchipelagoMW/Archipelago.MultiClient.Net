using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    class IntegrationTest
    {
        [Test]
        public void Integration_Test()
        {
            var session = ArchipelagoSessionFactory.CreateSession("localhost");

            session.TryConnectAndLogin("Archipelago", "Spectator", new Version(0, 2, 4), ItemsHandlingFlags.NoItems,
                new List<string>{ "IgnoreGame", "TextOnly" });

            //int nope = session.DataStorage["NotExisting"];

            //session.DataStorage["KeyA"] = 10;

            object y = null;

            session.DataStorage["KeyA"].OnValueChanged += (value, newValue) =>
            {
                y = (int)newValue;
            };



            
            /*Expression<Func<int, int>> test = k => (k - 20) << 0;

            session.DataStorage["Func"] = (Expression<Func<int, int>>)w => (w - 20) << 0;

            session.DataStorage["KeyA"] += 10 * 5;*/


            //session.DataStorage["KeyA"]++;

            //session.DataStorage["KeyB"] = "hello";

            //session.DataStorage["KeyC"] += new { A = 10, B = "Test" };


            //int resultA = session.DataStorage["KeyA"];
            //string resultB = session.DataStorage["KeyB"];

            while (y == null)
                Thread.Sleep(10);

            var x = 10;
        }
    }
}
