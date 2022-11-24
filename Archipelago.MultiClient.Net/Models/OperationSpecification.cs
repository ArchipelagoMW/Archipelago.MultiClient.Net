using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
#if USE_OCULUS_NEWTONSOFT
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using Oculus.Newtonsoft.Json.Linq;
using Oculus.Newtonsoft.Json.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
#endif

namespace Archipelago.MultiClient.Net.Models
{
    public class OperationSpecification
    {
        [JsonProperty("operation")]
#if !USE_OCULUS_NEWTONSOFT
        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
#endif
#if USE_OCULUS_NEWTONSOFT
        public string Operation;
#else
        public Operation Operation;
#endif

        [JsonProperty("value")]
        public JToken Value { get; set; }

        public override string ToString() => $"{Operation}: {Value}";
    }

    public class Bitwise
    {
        public static OperationSpecification Xor(long i) =>
#if USE_OCULUS_NEWTONSOFT
            new OperationSpecification { Operation = Operation.Xor.ToString(), Value = i };
#else
            new OperationSpecification { Operation = Operation.Xor, Value = i };
#endif
        public static OperationSpecification Or(long i) =>
#if USE_OCULUS_NEWTONSOFT
            new OperationSpecification { Operation = Operation.Or.ToString(), Value = i };
#else
            new OperationSpecification { Operation = Operation.Or, Value = i };
#endif

        public static OperationSpecification And(long i) =>
#if USE_OCULUS_NEWTONSOFT
            new OperationSpecification { Operation = Operation.And.ToString(), Value = i };
#else
            new OperationSpecification { Operation = Operation.And, Value = i };
#endif

        public static OperationSpecification LeftShift(long i) =>
#if USE_OCULUS_NEWTONSOFT
            new OperationSpecification { Operation = "left_shift", Value = i };
#else
            new OperationSpecification { Operation = Operation.LeftShift, Value = i };
#endif

        public static OperationSpecification RightShift(long i) =>
#if USE_OCULUS_NEWTONSOFT
            new OperationSpecification { Operation = "right_shift", Value = i };
#else
            new OperationSpecification { Operation = Operation.RightShift, Value = i };
#endif
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