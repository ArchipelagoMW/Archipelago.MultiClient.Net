using Archipelago.MultiClient.Net.Cache;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Tests.Stubs;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Archipelago.MultiClient.Net.Tests
{
    [TestFixture]
    public class DataPackageFileSystemCacheFixture
    {
        [Test]
        public void Should_request_data_package_when_no_local_cache_is_available()
        {
            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

            _ = new DataPackageFileSystemCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(new RoomInfoPacket {
                DataPackageVersion = 1,
                DataPackageVersions = new Dictionary<string, int> {
                    { "One", 1 }
                }
            });

            socket.Received().SendPacket(Arg.Any<GetDataPackagePacket>());
        }

        [Test]
        public void Should_not_request_data_when_local_version_is_same_as_server_version()
        {
            var localDataPackage = new DataPackage {
                Version = 7, Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData(3) }, 
                    { "Two", new TestGameData(4) }
                }
            };

            var roomInfo = new RoomInfoPacket {
                DataPackageVersion = 7,
                DataPackageVersions = new Dictionary<string, int> {
                    { "One", 3 },
                    { "Two", 4 }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(out _)
                .Returns(x => {
                    x[0] = localDataPackage;
                    return true;
                });

            _ = new DataPackageFileSystemCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(roomInfo);

            socket.DidNotReceive().SendPacket(Arg.Any<GetDataPackagePacket>());
        }

        [Test]
        public void Should_only_request_data_for_games_that_a_have_different_version_then_cached()
        {
            var localDataPackage = new DataPackage
            {
                Version = 15,
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData(3) },
                    { "Two", new TestGameData(7) },
                    { "Three", new TestGameData(5) }
                }
            };

            var roomInfo = new RoomInfoPacket
            {
                DataPackageVersion = 15,
                DataPackageVersions = new Dictionary<string, int> {
                    { "One", 3 },
                    { "Two", 6 },
                    { "Three", 6 }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(out _)
                .Returns(x => {
                    x[0] = localDataPackage;
                    return true;
                });

            _ = new DataPackageFileSystemCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(roomInfo);

            socket.Received().SendPacket(Arg.Is<GetDataPackagePacket>(p =>
                p.Exclusions.Length == 1 && p.Exclusions[0] == "One"
            ));
        }

        [Test]
        public void Should_save_received_datapackage_contents()
        {
            var serverDataPackage = new DataPackagePacket {
                DataPackage = new DataPackage {
                    Version = 2, Games = new Dictionary<string, GameData> {
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
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();

            _ = new DataPackageFileSystemCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(serverDataPackage);

            fileSystemDataPackageProvider.Received().SaveDataPackageToFile(Arg.Is<DataPackage>(p => 
                p.Version == 2 
                    && p.Games.Count == 1
                    && p.Games["One"].Version == 2
                    && p.Games["One"].ItemLookup.Count == 1
                    && p.Games["One"].ItemLookup["ItemOne"] == 101
                    && p.Games["One"].LocationLookup.Count == 2
                    && p.Games["One"].LocationLookup["LocationOne"] == 201
                    && p.Games["One"].LocationLookup["LocationTwo"] == 202
            ));
        }

        [Test]
        public void Should_merge_received_datapackage_with_cached_version()
        {
            var localDataPackage = new DataPackage {
                Version = 4,
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData(1) }, 
                    { "Two", new TestGameData(3) }
                }
            };
            
            var serverDataPackage = new DataPackagePacket {
                DataPackage = new DataPackage {
                    Version = 5,
                    Games = new Dictionary<string, GameData> {
                        { "One", new TestGameData(2) }
                    }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(out _)
                .Returns(x => {
                    x[0] = localDataPackage;
                    return true;
                });

            var sut = new DataPackageFileSystemCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(serverDataPackage);

            fileSystemDataPackageProvider.Received().SaveDataPackageToFile(Arg.Is<DataPackage>(p =>
                p.Version == 5
                && p.Games.Count == 2
                && p.Games["One"].Version == 2
                && p.Games["Two"].Version == 3
            ));

            sut.TryGetDataPackageFromCache(out var inMemoryDataPackage);

            Assert.IsTrue(inMemoryDataPackage.Version == 5
                          && inMemoryDataPackage.Games.Count == 2
                          && inMemoryDataPackage.Games["One"].Version == 2
                          && inMemoryDataPackage.Games["Two"].Version == 3);
        }

        [Test]
        public void Should_not_save_game_when_version_is_0_but_still_keep_it_in_memory()
        {
            var localDataPackage = new DataPackage
            {
                Version = 2,
                Games = new Dictionary<string, GameData> {
                    { "One", new TestGameData(2) }
                }
            };

            var serverDataPackage = new DataPackagePacket
            {
                DataPackage = new DataPackage
                {
                    Version = 0,
                    Games = new Dictionary<string, GameData> {
                        { "One", new TestGameData(3) },
                        { "Two", new TestGameData(0) }
                    }
                }
            };

            var socket = Substitute.For<IArchipelagoSocketHelper>();
            var fileSystemDataPackageProvider = Substitute.For<IFileSystemDataPackageProvider>();
            fileSystemDataPackageProvider.TryGetDataPackage(out _)
                .Returns(x => {
                    x[0] = localDataPackage;
                    return true;
                });

            var sut = new DataPackageFileSystemCache(socket, fileSystemDataPackageProvider);

            socket.PacketReceived += Raise.Event<ArchipelagoSocketHelper.PacketReceivedHandler>(serverDataPackage);

            fileSystemDataPackageProvider.Received().SaveDataPackageToFile(Arg.Is<DataPackage>(p =>
                p.Games.Count == 1
                && p.Games["One"].Version == 3
            ));

            sut.TryGetDataPackageFromCache(out var inMemoryDataPackage);

            Assert.IsTrue(inMemoryDataPackage.Version == 0
                          && inMemoryDataPackage.Games.Count == 2
                          && inMemoryDataPackage.Games["One"].Version == 3
                          && inMemoryDataPackage.Games["Two"].Version == 0);
        }
    }
}
