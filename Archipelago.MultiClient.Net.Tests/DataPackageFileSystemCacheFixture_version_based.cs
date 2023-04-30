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
	//use Directory C:\Users\current_user\AppData\Local\Archipelago\Cache 
	//retention 1 month since last moddified, update File.SetLastWriteTime(file, modifiedTime) in use


	[TestFixture]
    public class DataPackageFileSystemCacheFixture_version_based
    {
	    [Test]
	    public void Should_use_version_based_file_system_provider()
	    {
		    var socket = Substitute.For<IArchipelagoSocketHelper>();

		    var sut = new DataPackageCache(socket);

		    socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(new RoomInfoPacket
		    {
			    Version = new NetworkVersion(0, 3, 7),
			    Games = Array.Empty<string>()
		    });

		    Assert.That(sut.FileSystemDataPackageProvider, Is.InstanceOf<FileSystemVersionBasedDataPackageProvider>());
	    }


		[Test]
        public void Should_request_data_package_when_no_local_cache_is_available()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), "", out _)
	            .Returns(x =>
	            {
		            x[2] = null;
		            return false;
	            });

			_ = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(new RoomInfoPacket {
	            Version = new NetworkVersion(0, 3, 7),
				Games = new []{ "One" }
            });

            socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p =>
                p.Games.Length == 2 && p.Games[0] == "One" && p.Games[1] == "Archipelago"
            ));
        }

        [Test]
        public void Should_request_updates_for_archipelago_even_if_its_being_played()
        {
            var localDataPackage = new DataPackage
            {
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData(1) },
                    { "Archipelago", new TestGameData(1) }
                }
            };

            var roomInfo = new RoomInfoPacket
            {
	            Version = new NetworkVersion(0, 3, 7),
				Games = new[] { "One" },
                DataPackageVersions = new Dictionary<string, int> {
                    { "One", 1 },
                    { "Archipelago", 2 }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), "", out _)
                .Returns(x => {
                    x[2] = localDataPackage.Games[x.ArgAt<string>(0)];
                    return true;
                });

            _ = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);

            socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p =>
                p.Games.Length == 1 && p.Games[0] == "Archipelago"
            ));
        }

        [Test]
        public void Should_not_request_data_when_local_version_is_same_as_server_version()
        {
            var localDataPackage = new DataPackage {
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData(3) },
                    { "Two", new TestGameData(4) },
                    { "Archipelago", new TestGameData(1) }
                }
            };

            var roomInfo = new RoomInfoPacket {
	            Version = new NetworkVersion(0, 3, 7),
				Games = new []{ "One", "Two" },
                DataPackageVersions = new Dictionary<string, int> {
                    { "One", 3 },
                    { "Two", 4 },
                    { "Five", 7 },
                    { "Archipelago", 1 }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), "", out _)
                .Returns(x => {
                    x[2] = localDataPackage.Games[x.ArgAt<string>(0)];
                    return true;
                });

            _ = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);

            socket.DidNotReceive().SendPacket(Arg.Any<GetDataPackagePacket>());
        }

        [Test]
        public void Should_only_request_data_for_games_that_a_have_different_version_then_cached()
        {
            var localDataPackage = new DataPackage
            {
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData(3) },
                    { "Two", new TestGameData(7) },
                    { "Three", new TestGameData(5) },
                    { "Archipelago", new TestGameData(1) }
                }
            };

            var roomInfo = new RoomInfoPacket
            {
	            Version = new NetworkVersion(0, 3, 7),
				Games = new[] { "One", "Two", "Three" },
                DataPackageVersions = new Dictionary<string, int> {
                    { "One", 3 },
                    { "Two", 6 },
                    { "Three", 6 },
                    { "Archipelago", 1 }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), "", out _)
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
                                Version = 2,
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
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();

            _ = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(serverDataPackage);

            fileSystemDataPackageProvider.Received().SaveDataPackageToFile("One", Arg.Is<GameData>(d => 
                d.Version == 2
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
                    { "One", new TestGameData(1) },
                    { "Two", new TestGameData(3) },
                    { "Archipelago", new TestGameData(3) },
                }
            };

            var roomInfo = new RoomInfoPacket
            {
	            Version = new NetworkVersion(0, 3, 7),
				Games = new[] { "One", "Two" },
                DataPackageVersions = new Dictionary<string, int> {
                    { "One", 1 },
                    { "Two", 6 },
                    { "Archipelago", 3 }
                }
            };

            var serverDataPackage = new DataPackagePacket
            {
                DataPackage = new DataPackage
                {
                    Games = new Dictionary<string, GameData> {
                        { "One", new TestGameData(2) }
                    }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), "", out _)
                .Returns(x => {
                    x[2] = localDataPackage.Games[x.ArgAt<string>(0)];
                    return true;
                });

            var sut = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);
            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(serverDataPackage);

            sut.TryGetDataPackageFromCache(out var inMemoryDataPackage);

            Assert.IsTrue(inMemoryDataPackage.Games.Count == 3
                          && inMemoryDataPackage.Games["One"].Version == 2
                          && inMemoryDataPackage.Games["Two"].Version == 3
                          && inMemoryDataPackage.Games["Archipelago"].Version == 3);
        }

        [Test]
        public void Should_not_save_game_when_version_is_0_but_still_keep_it_in_memory()
        {
            var localDataPackage = new DataPackage
            {
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData(2) }
                }
            };

            var serverDataPackage = new DataPackagePacket
            {
                DataPackage = new DataPackage
                {
                    Games = new Dictionary<string, GameData> {
                        { "One", new TestGameData(3) },
                        { "Two", new TestGameData(0) }
                    }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), "", out _)
                .Returns(x => {
                    x[2] = localDataPackage.Games[x.ArgAt<string>(0)];
                    return true;
                });

            var sut = new DataPackageCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(serverDataPackage);

            fileSystemDataPackageProvider.Received().SaveDataPackageToFile("One", Arg.Is<TestGameData>(d => d.Version == 3));
            fileSystemDataPackageProvider.DidNotReceive().SaveDataPackageToFile("Two", Arg.Any<TestGameData>());

            sut.TryGetGameDataFromCache("One", out var gameDataGameOne);
            sut.TryGetGameDataFromCache("Two", out var gameDataGameTwo);

            Assert.That(gameDataGameOne.Version, Is.EqualTo(3));
            Assert.That(gameDataGameTwo.Version, Is.EqualTo(0));
        }

        [Test]
        public void Should_request_data_package_for_all_games_when_versions_are_not_provided_on_the_roominfo()
        {
	        var roomInfo = new RoomInfoPacket
	        {
		        Version = new NetworkVersion(0, 3, 7),
				Games = new[] { "One", "Two" },
		        DataPackageVersions = null
	        };

			var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();
	        fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), "", out _)
		        .Returns(x => {
			        x[2] = new GameData();
			        return true;
		        });

			_ = new DataPackageCache(socket, fileSystemDataPackageProvider);

			socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);

			socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p =>
		        p.Games.Length == 3 && p.Games[0] == "One" && p.Games[1] == "Two" && p.Games[2] == "Archipelago"
	        ));
        }

        [Test]
        public void Should_request_data_package_for_games_the_server_fails_to_send_the_version_for()
        {
	        var roomInfo = new RoomInfoPacket
	        {
		        Version = new NetworkVersion(0, 3, 7),
				Games = new[] { "One", "Two" },
		        DataPackageVersions = new Dictionary<string, int> {
			        { "One", 0 }
				}
	        };

			var socket = Substitute.For<IArchipelagoSocketHelper>();
	        var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataVersionBasedPackageProvider>();
	        fileSystemDataPackageProvider.TryGetDataPackage(Arg.Any<string>(), "", out _)
		        .Returns(x => {
			        x[2] = new GameData();
			        return true;
		        });

	        _ = new DataPackageCache(socket, fileSystemDataPackageProvider);

	        socket.PacketReceived += Raise.Event<ArchipelagoSocketHelperDelagates.PacketReceivedHandler>(roomInfo);

	        socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p =>
		        p.Games.Length == 2 && p.Games[0] == "Two" && p.Games[1] == "Archipelago"
	        ));
        }
		class TestGameData : GameData
        {
	        public TestGameData(int version)
	        {
		        Version = version;
		        LocationLookup = new Dictionary<string, long>(0);
		        ItemLookup = new Dictionary<string, long>(0);
	        }
        }
	}
}
