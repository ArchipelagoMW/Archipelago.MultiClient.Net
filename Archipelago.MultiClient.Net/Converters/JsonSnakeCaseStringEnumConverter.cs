#if NET6_0_OR_GREATER
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Archipelago.MultiClient.Net.Converters
{
	public class JsonSnakeCaseStringEnumConverter : JsonStringEnumConverter
	{
		public JsonSnakeCaseStringEnumConverter() : base(SnakeCaseNamingPolicy.Instance)
		{
		}
	}

	public class SnakeCaseNamingPolicy : JsonNamingPolicy
	{
		public static SnakeCaseNamingPolicy Instance { get; } = new();

		public override string ConvertName(string name) => 
			string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
	}
}
#endif
