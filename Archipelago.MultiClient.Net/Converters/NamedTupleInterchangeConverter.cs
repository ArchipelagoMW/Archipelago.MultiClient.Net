using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Archipelago.MultiClient.Net.Converters
{
    public class NamedTupleInterchangeConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var objectType = value.GetType();
            var token = JObject.FromObject(value);
            token["class"] = objectType.Name;
            token.WriteTo(writer);
        }
    }
}
