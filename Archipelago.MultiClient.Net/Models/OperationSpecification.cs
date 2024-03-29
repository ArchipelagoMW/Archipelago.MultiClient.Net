﻿using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections;

namespace Archipelago.MultiClient.Net.Models
{
	/// <summary>
	/// An opperation to apply to the DataStorage
	/// </summary>
    public class OperationSpecification
    {
		/// <inheritdoc cref="T:Archipelago.MultiClient.Net.Models.OperationType"/>
		[JsonProperty("operation")]
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
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
	    public static OperationSpecification Min(JToken i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = i };

		/// <summary>
		/// Performs a Math.Max() on the store its current value vs the provided value
		/// </summary>
		/// <param name="i">The value to compare to</param>
		public static OperationSpecification Max(JToken i) =>
		    new OperationSpecification { OperationType = OperationType.Max, Value = i };

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
		public static OperationSpecification Pop(JToken value) =>
		    new OperationSpecification { OperationType = OperationType.Pop, Value = value };

		/// <summary>
		/// Performs Dictionary merge, adding all keys from value to the original dict overriding existing keys
		/// </summary>
		/// <param name="dictionary">The dictionary to merge in</param>
		public static OperationSpecification Update(IDictionary dictionary) =>
		    new OperationSpecification { OperationType = OperationType.Update, Value = JObject.FromObject(dictionary) };
	}

	/// <summary>
	/// Bitwise operations to apply to the DataStorage
	/// </summary>
	public static class Bitwise
    {
        public static OperationSpecification Xor(long i) =>
            new OperationSpecification { OperationType = OperationType.Xor, Value = i };

        public static OperationSpecification Or(long i) =>
            new OperationSpecification { OperationType = OperationType.Or, Value = i };

        public static OperationSpecification And(long i) =>
            new OperationSpecification { OperationType = OperationType.And, Value = i };

        public static OperationSpecification LeftShift(long i) =>
            new OperationSpecification { OperationType = OperationType.LeftShift, Value = i };

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
}