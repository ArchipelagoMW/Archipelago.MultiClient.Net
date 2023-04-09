using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Tests
{
	//use Directory C:\Users\<current_user>\AppData\Local\Archipelago\Cache\<game>\<checksum>.json
	//retention 1 month since last moddified, update File.SetLastWriteTime(file, modifiedTime) in use


	[TestFixture]
    public class DataPackageFileSystemCacheFixture
    {
	    [Test]
	    public void Should_use_checksum_base_file_system_provider()
	    {
		    var socket = Substitute.For<IArchipelagoSocketHelper>();

		    var sut = new DataPackageCache(socket);

		    socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(new RoomInfoPacket
		    {
			    Version = new NetworkVersion(0, 4, 0),
			    Games = Array.Empty<string>()
		    });

		    Assert.That(sut.FileSystemDataPackageProvider, Is.InstanceOf<FileSystemCheckSumDataPackageProvider>());
	    }
		
		[Test]
        public void Should_request_data_package_when_no_local_cache_is_available()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage("", "", out _)
	            .ReturnsForAnyArgs(x => {
		            x[2] = null;
		            return true;
	            });

			_ = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(new RoomInfoPacket {
				Version = new NetworkVersion(0, 4, 0),
                Games = new []{ "One" }
            });

            socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p =>
                p.Games.Length == 1 && p.Games[0] == "One"
            ));
        }

        [Test]
        public void Should_not_request_data_when_local_contains_a_file_with_same_checksum()
        {
            var localDataPackage = new DataPackage {
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData("3") },
                    { "Two", new TestGameData("4") },
                    { "Archipelago", new TestGameData("1") }
                }
            };

            var roomInfo = new RoomInfoPacket {
	            Version = new NetworkVersion(0, 4, 0),
				Games = new []{ "One", "Two", "Archipelago" },
                DataPackageChecksums = new Dictionary<string, string> {
                    { "One", "3" },
                    { "Two", "4" },
                    { "Archipelago", "1" }
                }
            };

			var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage("", "", out _)
	            .ReturnsForAnyArgs(x => {
		            x[2] = localDataPackage.Games[x.ArgAt<string>(0)];
		            return true;
	            });

			_ = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);

            socket.DidNotReceive().SendPacket(Arg.Any<GetDataPackagePacket>());
        }

		[Test]
        public void Should_only_request_data_for_games_that_a_have_different_checksum_then_cached()
        {
            var localDataPackage = new DataPackage
            {
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData("3") },
                    { "Two", new TestGameData("7") },
                    { "Three", new TestGameData("5") },
                    { "Archipelago", new TestGameData("1") }
                }
            };

            var roomInfo = new RoomInfoPacket
            {
	            Version = new NetworkVersion(0, 4, 0),
				Games = new[] { "One", "Two", "Three" },
                DataPackageChecksums = new Dictionary<string, string> {
                    { "One", "3" },
                    { "Two", "6" },
                    { "Three", "6" },
                    { "Archipelago", "1" }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), Arg.Any<string>(), out _)
                .Returns(x => {
                    x[2] = localDataPackage.Games[x.ArgAt<string>(0)];
                    return true;
                });

            _ = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);

            socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p =>
                p.Games.Length == 2 && p.Games[0] == "Two" && p.Games[1] == "Three"
            ));
        }

        [Test]
        public void Should_save_received_datapackage_contents()
        {
            var serverDataPackage = new DataPackagePacket {
                DataPackage = new DataPackage {
                    Games = new Dictionary<string, GameData> {
                        { 
                            "One", new GameData {
                                Checksum = "2",
                                ItemLookup = new Dictionary<string, long> {
                                    { "ItemOne", 101 }
                                },
                                LocationLookup = new Dictionary<string, long> {
                                    { "LocationOne", 201 },
                                    { "LocationTwo", 202 }
                                }
                            }
                        }
                    }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

            _ = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(serverDataPackage);

            fileSystemDataPackageProvider.Received().SaveDataPackageToFile("One", Arg.Is<GameData>(d => 
                d.Checksum == "2"
                    && d.ItemLookup.Count == 1
                    && d.ItemLookup["ItemOne"] == 101
                    && d.LocationLookup.Count == 2
                    && d.LocationLookup["LocationOne"] == 201
                    && d.LocationLookup["LocationTwo"] == 202
            ));
        }

        [Test]
        public void Should_merge_received_datapackage_with_cached_version()
        {
            var localDataPackage = new DataPackage
            {
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData("1") },
                    { "Two", new TestGameData("3") },
                    { "Archipelago", new TestGameData("3") },
                }
            };

            var roomInfo = new RoomInfoPacket
            {
	            Version = new NetworkVersion(0, 4, 0),
				Games = new[] { "One", "Two", "Archipelago" },
                DataPackageChecksums = new Dictionary<string, string> {
                    { "One", "1" },
                    { "Two", "6" },
                    { "Archipelago", "3" }
                }
            };

            var serverDataPackage = new DataPackagePacket
            {
                DataPackage = new DataPackage
                {
                    Games = new Dictionary<string, GameData> {
                        { "Two", new TestGameData("6") }
                    }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage("", "", out _)
	            .ReturnsForAnyArgs(x => {
		            x[2] = localDataPackage.Games[x.ArgAt<string>(0)];
		            return true;
	            });

			var sut = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(serverDataPackage);

            sut.TryGetDataPackageFromCache(out var inMemoryDataPackage);

            Assert.IsTrue(inMemoryDataPackage.Games.Count == 3
                          && inMemoryDataPackage.Games["One"].Checksum == "1"
                          && inMemoryDataPackage.Games["Two"].Checksum == "6"
                          && inMemoryDataPackage.Games["Archipelago"].Checksum == "3");
        }

        [Test]
        public void Should_request_data_package_for_games_the_server_fails_to_send_the_version_for()
        {
	        var localDataPackage = new DataPackage
	        {
		        Games = new Dictionary<string, GameData> {
			        { "One", new TestGameData("1") },
			        { "Two", new TestGameData("3") },
		        }
	        };

			var roomInfo = new RoomInfoPacket
	        {
		        Version = new NetworkVersion(0, 4, 0),
				Games = new[] { "One", "Two" },
		        DataPackageChecksums = new Dictionary<string, string> {
			        { "One", "1" }
				}
	        };

			var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
	        fileSystemDataPackageProvider.TryGetDataPackage("", "", out _)
		        .ReturnsForAnyArgs(x => {
					x[2] = localDataPackage.Games[x.ArgAt<string>(0)];
					return true;
		        });

			_ = new DataPackageCache(socket, fileSystemDataPackageProvider);

	        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);

			socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p => Checkpacket(p)));

			//socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p =>
		    //    p.Games.Length == 1 && p.Games[0] == "Two"
	        //));
        }

        bool Checkpacket(GetDataPackagePacket p)
        {
	        return p.Games.Length == 1 && p.Games[0] == "Two";
        }

        class TestGameData : GameData
        {
	        public TestGameData(string checksum)
	        {
		        Checksum = checksum;
		        LocationLookup = new Dictionary<string, long>(0);
		        ItemLookup = new Dictionary<string, long>(0);
	        }
        }
	}
}
