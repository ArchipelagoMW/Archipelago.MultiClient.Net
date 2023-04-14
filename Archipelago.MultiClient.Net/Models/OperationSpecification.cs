using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections;

namespace Archipelago.MultiClient.Net.Models
{
    public class OperationSpecification
    {
        [JsonProperty("operation")]
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public OperationType OperationType;

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
		public static OperationSpecification Min(JToken i) =>
			new OperationSpecification { OperationType = OperationType.Min, Value = i };

		public static OperationSpecification Max(JToken i) =>
		    new OperationSpecification { OperationType = OperationType.Max, Value = i };

	    public static OperationSpecification Remove(JToken t) =>
		    new OperationSpecification { OperationType = OperationType.Max, Value = t };

	    public static OperationSpecification Pop(JToken t) =>
		    new OperationSpecification { OperationType = OperationType.Max, Value = t };

	    public static OperationSpecification Update(IDictionary dictionary) =>
		    new OperationSpecification { OperationType = OperationType.Max, Value = JObject.FromObject(dictionary) };
	}


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