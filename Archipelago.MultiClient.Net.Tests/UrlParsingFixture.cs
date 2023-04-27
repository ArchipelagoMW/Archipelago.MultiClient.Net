using NUnit.Framework;

namespace Archipelago.MultiClient.Net.Tests
{
	[TestFixture]
	class ArchipelagoSessionFactoryFixture
	{
		[TestCase("localhost", null, "unspecified://localhost:38281/")]
		[TestCase("localhost", 2000, "unspecified://localhost:2000/")]
		[TestCase("localhost:1234", 2000, "unspecified://localhost:1234/")]
		[TestCase("archipelago.gg", null, "unspecified://archipelago.gg:38281/")]
		[TestCase("archipelago.gg", 38281, "unspecified://archipelago.gg:38281/")]
		[TestCase("ws://archipelago.gg", 38281, "ws://archipelago.gg:38281/")]
		[TestCase("ws://archipelago.gg:999", null, "ws://archipelago.gg:999/")]
		[TestCase("wss://somehost.com:5000", null, "wss://somehost.com:5000/")]
		[TestCase("wss://somehost.com", 5000, "wss://somehost.com:5000/")]
		[TestCase("somehost.com", 4000, "unspecified://somehost.com:4000/")]
		public void Should_parse_url(string url, int? port, string expectedUri)
		{
			var uri = ArchipelagoSessionFactory.ParseUri(url, port ?? 38281);

			Assert.That(uri.ToString(), Is.EqualTo(expectedUri));
		}
	}
}
