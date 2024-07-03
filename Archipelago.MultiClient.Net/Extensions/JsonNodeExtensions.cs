
using System;
using System.Collections.Generic;
using System.Data;

#if NET6_0_OR_GREATER
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
#else
using Newtonsoft.Json.Linq;
#endif

namespace Archipelago.MultiClient.Net.Extensions
{
	static class JsonNodeExtensions
	{
#if NET6_0_OR_GREATER
		public static T ToObject<T>(this JsonNode node)
		{

			//TODO

			return default;
		}

		internal static void Merge(this JsonArray arrayA, JsonNode node)
		{
			if (node is not JsonArray arrayB)
				return;

			arrayA.AddRange(arrayB);
		}

		internal static void AddRange(this JsonArray array, IList<JsonNode> nodes)
		{
			foreach (var node in nodes)
				array.Add(node);
		}
#endif

#if NET6_0_OR_GREATER
		public static bool IsNumber(this JsonNode node)
		{
			if (node is not JsonValue)
				return false;

			var value = node.GetValue<object>();
			if (value is JsonElement element)
			{
				switch (element.ValueKind)
				{
					case JsonValueKind.Number:
						return true;

					default:
						return false;
				}
			}

			var valueTypeType = value.GetType();
			valueTypeType = Nullable.GetUnderlyingType(valueTypeType) ?? valueTypeType;

			if (valueTypeType == typeof(BigInteger))
				return true;
			
			switch (Type.GetTypeCode(valueTypeType))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}
#else
		public static bool IsNumber(this JToken token) => token.Type == JTokenType.Integer;
#endif
#if NET6_0_OR_GREATER
		public static bool IsArray(this JsonNode node) => node is JsonArray;
#else
		public static bool IsArray(this JToken token) => token.Type == JTokenType.Array;
#endif

#if NET6_0_OR_GREATER
		public static bool IsNull(this JsonNode node) => node is null;
#else
		public static bool IsNull(this JToken token) => token.Type == JTokenType.Null;
#endif
	}
}



