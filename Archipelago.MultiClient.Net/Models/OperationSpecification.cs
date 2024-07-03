using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using System.Collections;

#if !NET35
using System.Numerics;
#endif

#if NET6_0_OR_GREATER
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using JToken = System.Text.Json.Nodes.JsonNode;
using JObject = System.Text.Json.Nodes.JsonObject;
using System.Text.Json.Serialization;
using Archipelago.MultiClient.Net.Converters;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
#endif

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Archipelago.MultiClient.Net.Models
{
	/// <summary>
	/// An opperation to apply to the DataStorage
	/// </summary>
    public class OperationSpecification
    {
		/// <inheritdoc cref="T:Archipelago.MultiClient.Net.Models.OperationType"/>
		[JsonProperty("operation")]
#if NET6_0_OR_GREATER
        [JsonConverter(typeof(JsonSnakeCaseStringEnumConverter))]
#else
		[JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
#endif
		public OperationType OperationType;

		/// <summary>
		/// The value related to this operation
		/// </summary>
        [JsonProperty("value")]
        public JToken Value { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{OperationType}: {Value}";
    }

	/// <summary>
	/// Operations to apply to the DataStorage
	/// </summary>
    public static class Operation
    {
		/// <summary>
		/// Performs a Math.Min() on the store its current value vs the provided value
		/// </summary>
		/// <param name="i">The value to compare to</param>
		public static OperationSpecification Min(int i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = i };
		/// <inheritdoc cref="Min(int)"/>
		public static OperationSpecification Min(long i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = i };
		/// <inheritdoc cref="Min(int)"/>
		public static OperationSpecification Min(float i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = i };
		/// <inheritdoc cref="Min(int)"/>
		public static OperationSpecification Min(double i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = i };
		/// <inheritdoc cref="Min(int)"/>
		public static OperationSpecification Min(decimal i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = i };
		/// <inheritdoc cref="Min(int)"/>
		public static OperationSpecification Min(JToken i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = i };
#if !NET35
		/// <inheritdoc cref="Min(int)"/>
		public static OperationSpecification Min(BigInteger i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = JToken.Parse(i.ToString()) };
#endif

		/// <summary>
		/// Performs a Math.Max() on the store its current value vs the provided value
		/// </summary>
		/// <param name="i">The value to compare to</param>
		public static OperationSpecification Max(int i) =>
			new OperationSpecification { OperationType = OperationType.Max, Value = i };
		/// <inheritdoc cref="Max(int)"/>
		public static OperationSpecification Max(long i) =>
			new OperationSpecification { OperationType = OperationType.Max, Value = i };
		/// <inheritdoc cref="Max(int)"/>
		public static OperationSpecification Max(float i) =>
			new OperationSpecification { OperationType = OperationType.Max, Value = i };
		/// <inheritdoc cref="Max(int)"/>
		public static OperationSpecification Max(double i) =>
			new OperationSpecification { OperationType = OperationType.Max, Value = i };
		/// <inheritdoc cref="Max(int)"/>
		public static OperationSpecification Max(decimal i) =>
			new OperationSpecification { OperationType = OperationType.Max, Value = i };
		/// <inheritdoc cref="Max(int)"/>
		public static OperationSpecification Max(JToken i) =>
		    new OperationSpecification { OperationType = OperationType.Max, Value = i };
#if !NET35
		/// <inheritdoc cref="Max(int)"/>
		public static OperationSpecification Max(BigInteger i) =>
			new OperationSpecification { OperationType = OperationType.Max, Value = JToken.Parse(i.ToString()) };
#endif

		/// <summary>
		/// Performs a List.Remove() to remove the first occurrence of the provided value
		/// </summary>
		/// <param name="value">The value to remove</param>
		public static OperationSpecification Remove(JToken value) =>
		    new OperationSpecification { OperationType = OperationType.Remove, Value = value };

		/// <summary>
		/// Performs a List.RemoveAt() or Dictionary.Remove() to remove a specified index or key from a list or dictionary
		/// </summary>
		/// <param name="value">The index or key to remove</param>
		public static OperationSpecification Pop(int value) =>
			new OperationSpecification { OperationType = OperationType.Pop, Value = value };
		/// <inheritdoc cref="Pop(int)"/>
		public static OperationSpecification Pop(JToken value) =>
		    new OperationSpecification { OperationType = OperationType.Pop, Value = value };


		/// <summary>
		/// Performs Dictionary merge, adding all keys from value to the original dict overriding existing keys
		/// </summary>
		/// <param name="dictionary">The dictionary to merge in</param>
		public static OperationSpecification Update(IDictionary dictionary) =>
		    new OperationSpecification { OperationType = OperationType.Update, Value = CreateFromDictionary(dictionary) };

		/// <summary>
		/// Performs a Math.Floor() on the store its current value
		/// </summary>
		public static OperationSpecification Floor() =>
			new OperationSpecification { OperationType = OperationType.Floor, Value = null };

		/// <summary>
		/// Performs a Math.Ceiling() on the store its current value
		/// </summary>
		public static OperationSpecification Ceiling() =>
			new OperationSpecification { OperationType = OperationType.Ceil, Value = null };

#if NET6_0_OR_GREATER
		static JToken CreateFromDictionary(IDictionary dictionary)
		{
			var obj = new JObject();

			foreach (DictionaryEntry dictionaryEntry in dictionary)
			{
				//fix if key isnt a string
				obj.Add((string)dictionaryEntry.Key, JToken.Create(dictionaryEntry.Value));
			}

			return obj;
		}
#else
		static JToken CreateFromDictionary(IDictionary dictionary) => JObject.FromObject(dictionary);
#endif
	}

	/// <summary>
	/// Bitwise operations to apply to the DataStorage
	/// </summary>
	public static class Bitwise
    {
		/// <summary>
		/// Performs a bitwise Exclusive OR on the store its current value vs the provided value
		/// </summary>
		/// <param name="i">The value to XOR with</param>
		public static OperationSpecification Xor(long i) =>
            new OperationSpecification { OperationType = OperationType.Xor, Value = i };
#if !NET35
		/// <inheritdoc cref="Xor(long)"/>
	    public static OperationSpecification Xor(BigInteger i) =>
		    new OperationSpecification { OperationType = OperationType.Xor, Value = JToken.Parse(i.ToString()) };
#endif

	    /// <summary>
	    /// Performs a bitwise OR on the store its current value vs the provided value
	    /// </summary>
	    /// <param name="i">The value to OR with</param>
		public static OperationSpecification Or(long i) =>
            new OperationSpecification { OperationType = OperationType.Or, Value = i };
#if !NET35
		/// <inheritdoc cref="Or(long)"/>
		public static OperationSpecification Or(BigInteger i) =>
			new OperationSpecification { OperationType = OperationType.Or, Value = JToken.Parse(i.ToString()) };
#endif

	    /// <summary>
	    /// Performs a bitwise AND on the store its current value vs the provided value
	    /// </summary>
	    /// <param name="i">The value to AND with</param>
		public static OperationSpecification And(long i) =>
            new OperationSpecification { OperationType = OperationType.And, Value = i };
#if !NET35
	    /// <inheritdoc cref="And(long)"/>
	    public static OperationSpecification And(BigInteger i) =>
		    new OperationSpecification { OperationType = OperationType.And, Value = JToken.Parse(i.ToString()) };
#endif
		/// <summary>
		/// Performs a bitwise left shift on the store its current value by the provided amount
		/// </summary>
		/// <param name="i">the amount to shift</param>
		public static OperationSpecification LeftShift(long i) =>
            new OperationSpecification { OperationType = OperationType.LeftShift, Value = i };

		/// <summary>
		/// Performs a bitwise right shift on the store its current value by the provided amount
		/// </summary>
		/// <param name="i">the amount to shift</param>
		public static OperationSpecification RightShift(long i) =>
            new OperationSpecification { OperationType = OperationType.RightShift, Value = i };
    }

	/// <summary>
	/// Provides a method to be called when a certain DataStorage operation completes
	/// </summary>
	public class Callback
    {
        internal DataStorageHelper.DataStorageUpdatedHandler Method { get; set; }

        Callback() { }

        /// <summary>
		/// Adds a callback to the current sequence of operations.
		/// This callback will be called only once after the server is done processing your operations.
		/// This is useful in-case you need to know the results of your operations when they depend on the current state of the server
		/// </summary>
		/// <param name="callback">The method to be called with the oldValue at the start of your operation and the newValue after your operation completed</param>
		public static Callback Add(DataStorageHelper.DataStorageUpdatedHandler callback) => 
            new Callback { Method = callback };
    }

	/// <summary>
	/// Provides a way to add additional arguments to the DataStorage operation
	/// additional arguments are send back by the server and can be checked in callbacks or handlers
	/// </summary>
	public class AdditionalArgument
	{
		internal string Key { get; set; }
		internal JToken Value { get; set; }

		AdditionalArgument() { }

		/// <summary>
		/// Adds an additional argument to the current datastorage operations.
		/// additional arguments are send back by the server and can be checked in callbacks or handlers
		/// </summary>
		/// <param name="name">The name of the argument</param>
		/// <param name="value">The value of the argument</param>
		public static AdditionalArgument Add(string name, JToken value) =>
			new AdditionalArgument { Key = name, Value = value };
	}
}