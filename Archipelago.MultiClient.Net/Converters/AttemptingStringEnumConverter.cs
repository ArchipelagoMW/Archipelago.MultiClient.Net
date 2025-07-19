using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

public class AttemptingStringEnumConverter : StringEnumConverter
{
	public AttemptingStringEnumConverter() : base() { }

	public AttemptingStringEnumConverter(Type namingStrategyType) : base(namingStrategyType) { }

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch (JsonSerializationException)
        {
	        return objectType.IsValueType ? Activator.CreateInstance(objectType) : null;
        }
    }
}
