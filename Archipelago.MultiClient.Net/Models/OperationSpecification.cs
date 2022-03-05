using Archipelago.MultiClient.Net.Enums;
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

        public override string ToString()
        {
            return $"{Operation}: {Value}";
        }
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
}