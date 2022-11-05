using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Archipelago.MultiClient.Net.Models
{
    public class OperationSpecification
    {
        [JsonProperty("operation")]
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public Operation Operation;

        [JsonProperty("value")]
        public JToken Value { get; set; }

        public override string ToString() => $"{Operation}: {Value}";
    }

    public class Bitwise
    {
        public static OperationSpecification Xor(long i) =>
            new OperationSpecification { Operation = Operation.Xor, Value = i };

        public static OperationSpecification Or(long i) =>
            new OperationSpecification { Operation = Operation.Or, Value = i };

        public static OperationSpecification And(long i) =>
            new OperationSpecification { Operation = Operation.And, Value = i };

        public static OperationSpecification LeftShift(long i) =>
            new OperationSpecification { Operation = Operation.LeftShift, Value = i };

        public static OperationSpecification RightShift(long i) =>
            new OperationSpecification { Operation = Operation.RightShift, Value = i };
    }

    public class Callback
    {
        internal DataStorageHelper.DataStorageUpdatedHandler Method { get; set; }

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